using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLDMAP.Application.Accounts;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Security;
using NLDMAP.Infrastructure.Administration;
using NLDMAP.Infrastructure.Persistence;

namespace NLDMAP.Infrastructure.Identity;

public sealed class CitizenAccounts(
    IServiceScopeFactory scopes,
    AccountEmailSender sender,
    ILogger<CitizenAccounts> logger) : ICitizenAccounts
{
    public async Task RegisterAsync(CitizenRegistration input)
    {
        Validate(input);
        var email = input.Email.Trim();
        var name = input.DisplayName.Trim();
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await using var transaction = await db.Database.BeginTransactionAsync();
        await LockWritesAsync(db);

        var normalizedEmail = users.NormalizeEmail(email);
        var now = DateTimeOffset.UtcNow;
        if (await db.AccountInvitations.AnyAsync(invitation =>
            invitation.NormalizedEmail == normalizedEmail && invitation.AcceptedAtUtc == null &&
            invitation.RevokedAtUtc == null && invitation.ExpiresAtUtc > now))
            throw RegistrationFailed();

        var user = new ApplicationUser { UserName = email, Email = email, DisplayName = name };
        if (!(await users.CreateAsync(user, input.Password)).Succeeded)
            throw RegistrationFailed();

        AdministrationStore.Ensure(await users.AddToRoleAsync(user, RoleNames.Guest));
        await transaction.CommitAsync();
    }

    public async Task<CitizenProfile?> GetProfileAsync(ActorSession actor)
    {
        await using var scope = scopes.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await FindSessionUserAsync(users, actor);
        if (user is null)
            return null;

        var roles = await users.GetRolesAsync(user);
        if (!RoleNames.IsCitizenAccount(roles))
            return null;

        return ToProfile(user, roles.Contains(RoleNames.Reporter, StringComparer.OrdinalIgnoreCase));
    }

    public async Task SendEmailConfirmationAsync(ActorSession actor)
    {
        await using var scope = scopes.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await RequireCitizenAsync(users, actor);
        if (user.EmailConfirmed)
            return;

        if (string.IsNullOrWhiteSpace(user.Email))
            throw new AdministrationException("La cuenta no tiene un correo para confirmar.");

        var code = await users.GenerateEmailConfirmationTokenAsync(user);
        try
        {
            await sender.SendAsync(user.Email, "Confirma tu correo en Búsqueda Tlaxcala",
                $"Copia este código completo en el formulario de perfil:\n\n{code}\n\nVence en 2 horas. Si no lo solicitaste, ignora este mensaje.");
        }
        catch (Exception exception) when (exception is SmtpException or InvalidOperationException or IOException)
        {
            logger.LogWarning("No se pudo enviar el correo de confirmación para el perfil público.");
            throw new AdministrationException("No se pudo enviar el correo. Inténtalo más tarde.");
        }
    }

    public async Task<CitizenProfile> CompleteProfileAsync(ActorSession actor, CitizenProfileInput input)
    {
        Validate(input);
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await using var transaction = await db.Database.BeginTransactionAsync();
        await LockWritesAsync(db);

        var user = await RequireCitizenAsync(users, actor);
        var isReporter = await users.IsInRoleAsync(user, RoleNames.Reporter);
        if (ToProfile(user, isReporter).CanCreateReports)
            return ToProfile(user, true);

        if (!user.EmailConfirmed)
        {
            if (string.IsNullOrWhiteSpace(input.EmailCode) ||
                !(await users.ConfirmEmailAsync(user, input.EmailCode.Trim())).Succeeded)
                throw new AdministrationException("El código de confirmación no es válido o venció.");
        }

        user.DisplayName = input.FullName.Trim();
        user.PhoneNumber = input.PhoneNumber.Trim();
        user.PhoneNumberConfirmed = false;
        AdministrationStore.Ensure(await users.UpdateAsync(user));
        if (!isReporter)
            AdministrationStore.Ensure(await users.AddToRoleAsync(user, RoleNames.Reporter));
        if (await users.IsInRoleAsync(user, RoleNames.Guest))
            AdministrationStore.Ensure(await users.RemoveFromRoleAsync(user, RoleNames.Guest));

        await transaction.CommitAsync();
        return ToProfile(user, true);
    }

    private static async Task<ApplicationUser?> FindSessionUserAsync(
        UserManager<ApplicationUser> users, ActorSession actor)
    {
        if (string.IsNullOrEmpty(actor.UserId) || string.IsNullOrEmpty(actor.SecurityStamp))
            return null;

        var user = await users.FindByIdAsync(actor.UserId);
        return user is { IsActive: true } && user.SecurityStamp == actor.SecurityStamp &&
               !await users.IsLockedOutAsync(user) ? user : null;
    }

    private static async Task<ApplicationUser> RequireCitizenAsync(
        UserManager<ApplicationUser> users, ActorSession actor)
    {
        var user = await FindSessionUserAsync(users, actor);
        if (user is null || !RoleNames.IsCitizenAccount(await users.GetRolesAsync(user)))
            throw new AdministrationException("Inicia sesión con una cuenta pública activa.");

        return user;
    }

    private static CitizenProfile ToProfile(ApplicationUser user, bool reporter) =>
        new(user.DisplayName, user.Email, user.EmailConfirmed,
            reporter ? RoleNames.Reporter : RoleNames.Guest,
            reporter && user.EmailConfirmed && !string.IsNullOrWhiteSpace(user.DisplayName) &&
            !string.IsNullOrWhiteSpace(user.PhoneNumber));

    private static Task LockWritesAsync(ApplicationDbContext db) =>
        db.Database.IsNpgsql()
            ? db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(724910031)")
            : Task.CompletedTask;

    private static void Validate(object input)
    {
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), errors, validateAllProperties: true))
            throw new AdministrationException(string.Join(" ", errors.Select(error => error.ErrorMessage)));
    }

    private static AdministrationException RegistrationFailed() =>
        new("No se pudo crear la cuenta. Revisa el correo y los requisitos de la contraseña.");
}
