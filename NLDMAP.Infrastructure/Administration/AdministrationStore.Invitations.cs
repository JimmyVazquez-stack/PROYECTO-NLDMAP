using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Security;
using NLDMAP.Domain.Accounts;
using NLDMAP.Infrastructure.Persistence;

namespace NLDMAP.Infrastructure.Administration;

public sealed partial class AdministrationStore
{
    internal static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    private static string NewToken() => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
    private static string InvitationStatus(AccountInvitation i) => i.AcceptedAtUtc.HasValue ? "Aceptada" :
        i.RevokedAtUtc.HasValue ? "Revocada" : i.ExpiresAtUtc <= DateTimeOffset.UtcNow ? "Vencida" : "Pendiente";

    public Task<IReadOnlyList<InvitationRow>> GetInvitationsAsync(ActorSession actor) =>
        RunAsync<IReadOnlyList<InvitationRow>>(actor, PermissionNames.InvitationsManage, false, async o =>
        (await o.Db.AccountInvitations.AsNoTracking().OrderByDescending(i => i.CreatedAtUtc).Take(100).ToListAsync())
            .Select(i => new InvitationRow(i.Id, i.Email, i.DisplayName, i.ExpiresAtUtc, InvitationStatus(i), i.DeliveryStatus)).ToArray());

    public async Task<string> InviteAsync(ActorSession actor, InvitationInput input)
    {
        var token = NewToken();
        // Validar el origen antes de crear una invitación que no pueda enviarse.
        var link = emailSender.Link("Account/AcceptInvitation", ("token", token));
        var invitation = await RunAsync(actor, PermissionNames.InvitationsManage, true, async o =>
        {
            if (!o.Access.Allows(PermissionNames.RolesAssign)) throw new AdministrationException("También necesitas el permiso Asignar roles.");
            if (await o.Users.FindByEmailAsync(input.Email) is not null) throw new AdministrationException("Ya existe una cuenta con ese correo.");
            var normalized = o.Users.NormalizeEmail(input.Email);
            var now = DateTimeOffset.UtcNow;
            if (await o.Db.AccountInvitations.AnyAsync(i => i.NormalizedEmail == normalized && i.AcceptedAtUtc == null && i.RevokedAtUtc == null && i.ExpiresAtUtc > now))
                throw new AdministrationException("Ya hay una invitación vigente. Usa Reenviar o Revocar.");
            var roles = await ValidateGrantAsync(o.Db, o.Access, input.RoleIds);
            var i = new AccountInvitation { Id = Guid.NewGuid(), Email = input.Email, NormalizedEmail = normalized!,
                DisplayName = input.DisplayName, CreatedById = actor.UserId, CreatedAtUtc = DateTimeOffset.UtcNow,
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(input.ValidDays), TokenHash = HashToken(token),
                RoleIdsJson = JsonSerializer.Serialize(roles.Select(r => r.Id)) };
            o.Db.AccountInvitations.Add(i);
            Audit(o.Db, o.Access, "Invitation.Created", "Invitation", i.Id.ToString(), i.Email,
                after: new { i.Email, i.ExpiresAtUtc, Roles = roles.Select(r => r.Name) });
            return i;
        });
        return await DeliverInvitationAsync(invitation, token, link);
    }

    public async Task<string> ResendInvitationAsync(ActorSession actor, Guid id)
    {
        var token = NewToken(); var link = emailSender.Link("Account/AcceptInvitation", ("token", token));
        var invitation = await RunAsync(actor, PermissionNames.InvitationsManage, true, async o =>
        {
            if (!o.Access.Allows(PermissionNames.RolesAssign)) throw new AdministrationException("También necesitas el permiso Asignar roles.");
            var i = await o.Db.AccountInvitations.FindAsync(id) ?? throw new AdministrationException("No se encontró la invitación.");
            if (i.AcceptedAtUtc.HasValue || i.RevokedAtUtc.HasValue) throw new AdministrationException("Esta invitación ya se cerró.");
            await ValidateGrantAsync(o.Db, o.Access, JsonSerializer.Deserialize<string[]>(i.RoleIdsJson)!);
            i.CreatedById = actor.UserId; i.TokenHash = HashToken(token); i.ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(2); i.DeliveryStatus = "Pendiente";
            Audit(o.Db, o.Access, "Invitation.Resent", "Invitation", i.Id.ToString(), i.Email, after: new { i.ExpiresAtUtc });
            return i;
        });
        return await DeliverInvitationAsync(invitation, token, link);
    }

    public Task RevokeInvitationAsync(ActorSession actor, Guid id) => RunAsync(actor, PermissionNames.InvitationsManage, true, async o =>
    {
        var i = await o.Db.AccountInvitations.FindAsync(id) ?? throw new AdministrationException("No se encontró la invitación.");
        if (i.AcceptedAtUtc.HasValue || i.RevokedAtUtc.HasValue) throw new AdministrationException("Esta invitación ya se cerró.");
        i.RevokedAtUtc = DateTimeOffset.UtcNow;
        Audit(o.Db, o.Access, "Invitation.Revoked", "Invitation", i.Id.ToString(), i.Email); return true;
    });

    private async Task<string> DeliverInvitationAsync(AccountInvitation i, string token, string link)
    {
        var delivered = false;
        try { await emailSender.SendAsync(i.Email, "Invitación a NLDMAP",
            $"Hola {i.DisplayName}.\n\nAbre este enlace para definir tu contraseña:\n{link}\n\nVence el {i.ExpiresAtUtc:yyyy-MM-dd HH:mm} UTC. Si no esperabas esta invitación, ignora el mensaje."); delivered = true; }
        catch (Exception ex) when (ex is System.Net.Mail.SmtpException or InvalidOperationException or IOException) { /* La pantalla ofrece reintentar. Nunca registrar el token. */ }
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // Un envío lento no debe sobrescribir el estado de un reenvío más reciente.
        var hash = HashToken(token);
        var deliveryStatus = delivered ? "Enviada" : "Error de envío";
        await db.AccountInvitations.Where(x => x.Id == i.Id && x.TokenHash == hash)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.DeliveryStatus, deliveryStatus));
        return delivered ? "Invitación enviada al medio de correo configurado." : "Invitación creada, pero falló el envío. Revisa el correo configurado y pulsa Reenviar.";
    }
}
