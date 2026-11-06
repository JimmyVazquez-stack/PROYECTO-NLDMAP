using System.Data;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLDMAP.Application.Accounts;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Security;
using NLDMAP.Infrastructure.Administration;
using NLDMAP.Infrastructure.Persistence;

namespace NLDMAP.Infrastructure.Identity;

public sealed class AccountLifecycle(IServiceScopeFactory scopeFactory, AccountEmailSender sender,
    ILogger<AccountLifecycle> logger) : IAccountLifecycle
{
    public async Task AcceptInvitationAsync(string token, string password)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > 200) throw InvalidLink();
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await using var tx = await db.Database.BeginTransactionAsync(db.Database.IsNpgsql() ? IsolationLevel.ReadCommitted : IsolationLevel.Serializable);
        if (db.Database.IsNpgsql()) await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(724910031)");
        var hash = AdministrationStore.HashToken(token);
        var invitation = await db.AccountInvitations.SingleOrDefaultAsync(i => i.TokenHash == hash);
        if (invitation is null || invitation.AcceptedAtUtc.HasValue || invitation.RevokedAtUtc.HasValue || invitation.ExpiresAtUtc <= DateTimeOffset.UtcNow)
            throw InvalidLink();
        var issuer = await users.FindByIdAsync(invitation.CreatedById);
        if (issuer is null || !issuer.IsActive) throw InvalidLink();
        var access = await AdministrationStore.AccessForUserAsync(db, issuer);
        if (!access.Allows(PermissionNames.InvitationsManage) || !access.Allows(PermissionNames.RolesAssign)) throw InvalidLink();
        var roles = await AdministrationStore.ValidateGrantAsync(db, access, JsonSerializer.Deserialize<string[]>(invitation.RoleIdsJson)!);
        if (await users.FindByEmailAsync(invitation.Email) is not null) throw InvalidLink();
        var user = new ApplicationUser
        {
            UserName = invitation.Email,
            Email = invitation.Email,
            DisplayName = invitation.DisplayName,
            EmailConfirmed = true,
            IsActive = true
        };
        AdministrationStore.Ensure(await users.CreateAsync(user, password));
        AdministrationStore.Ensure(await users.AddToRolesAsync(user, roles.Select(r => r.Name!)));
        invitation.AcceptedAtUtc = DateTimeOffset.UtcNow;
        AdministrationStore.Audit(db, new(user.Id, user.Email!, false, []), "Invitation.Accepted", "User", user.Id, user.Email!,
            after: new { InvitationId = invitation.Id, Roles = roles.Select(r => r.Name) });
        await db.SaveChangesAsync(); await tx.CommitAsync();
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 256) return;
        await using var scope = scopeFactory.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email.Trim());
        if (user is null || !user.IsActive) return;
        var token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await users.GeneratePasswordResetTokenAsync(user)));
        try
        {
            var link = sender.Link("Account/ResetPassword", ("email", user.Email!), ("token", token));
            await sender.SendAsync(user.Email!, "Restablecer contraseña de NLDMAP",
                $"Abre este enlace para definir una nueva contraseña:\n{link}\n\nEl enlace vence en 2 horas. Si no lo solicitaste, ignora este mensaje.");
        }
        catch (Exception ex) when (ex is System.Net.Mail.SmtpException or InvalidOperationException or IOException)
        { logger.LogError("No fue posible entregar el correo de recuperación. Revisa la configuración de correo."); }
        // Misma respuesta para correos existentes, inexistentes y fallos de entrega.
    }

    public async Task ResetPasswordAsync(string email, string token, string password)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > 4096 || string.IsNullOrWhiteSpace(email) || email.Length > 256) throw InvalidLink();
        string decoded;
        try { decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)); }
        catch (FormatException) { throw InvalidLink(); }
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await using var tx = await db.Database.BeginTransactionAsync(db.Database.IsNpgsql() ? IsolationLevel.ReadCommitted : IsolationLevel.Serializable);
        if (db.Database.IsNpgsql()) await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(724910031)");
        var user = await users.FindByEmailAsync(email.Trim());
        if (user is null || !user.IsActive) throw InvalidLink();
        var result = await users.ResetPasswordAsync(user, decoded, password);
        if (!result.Succeeded && result.Errors.Any(e => e.Code == "InvalidToken")) throw InvalidLink();
        AdministrationStore.Ensure(result);
        AdministrationStore.Audit(db, new(user.Id, user.Email!, false, []), "User.PasswordReset", "User", user.Id, user.Email!);
        await db.SaveChangesAsync(); await tx.CommitAsync();
    }
    private static AdministrationException InvalidLink() => new("El enlace no es válido, ya se utilizó o venció. Solicita uno nuevo.");
}
