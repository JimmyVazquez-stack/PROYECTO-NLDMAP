using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Security;

namespace NLDMAP.Infrastructure.Administration;

public sealed partial class AdministrationStore
{
    public Task<IReadOnlyList<RoleRow>> GetRolesAsync(ActorSession actor) => RunAsync<IReadOnlyList<RoleRow>>(actor, PermissionNames.Access, false, async o =>
    {
        if (!new[] { PermissionNames.RolesRead, PermissionNames.RolesAssign, PermissionNames.UsersRead, PermissionNames.InvitationsManage }.Any(o.Access.Allows))
            throw new AdministrationException("No puedes consultar los roles.");
        var roles = await o.Db.Roles.AsNoTracking().OrderBy(r => r.Name).ToArrayAsync();
        var claims = await o.Db.RoleClaims.Where(c => c.ClaimType == PermissionNames.ClaimType).ToListAsync();
        var counts = await o.Db.UserRoles.GroupBy(r => r.RoleId).Select(g => new { Id = g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Id, x => x.Count);
        return roles.Select(r => new RoleRow(r.Id, r.Name!, r.ConcurrencyStamp!, counts.GetValueOrDefault(r.Id),
            RoleNames.SystemRoles.Contains(r.Name), r.Name == RoleNames.Administrator ? PermissionNames.Catalog.Keys.ToArray() :
                claims.Where(c => c.RoleId == r.Id).Select(c => c.ClaimValue!).Distinct().Order().ToArray())).ToArray();
    });

    public Task SaveRoleAsync(ActorSession actor, RoleInput input) => RunAsync(actor, PermissionNames.RolesManage, true, async o =>
    {
        if (!o.Access.IsAdministrator && input.Permissions.Any(p => !o.Access.Allows(p)))
            throw new AdministrationException("No puedes conceder permisos superiores a los tuyos.");
        var role = input.Id is null ? new IdentityRole(input.Name) :
            await o.Roles.FindByIdAsync(input.Id) ?? throw new AdministrationException("El rol ya no existe.");
        var beforeName = role.Name;
        if (input.Id is not null)
        {
            CheckStamp(role.ConcurrencyStamp, input.ConcurrencyStamp);
            if (role.Name == RoleNames.Administrator) throw new AdministrationException("El rol administrador está protegido.");
            if (RoleNames.SystemRoles.Contains(role.Name) && role.Name != input.Name)
                throw new AdministrationException("Los roles del sistema conservan su nombre.");
        }
        if (string.Equals(input.Name, RoleNames.Administrator, StringComparison.OrdinalIgnoreCase))
            throw new AdministrationException("El rol administrador está reservado.");
        var previous = input.Id is null ? [] : (await o.Roles.GetClaimsAsync(role)).Where(c => c.Type == PermissionNames.ClaimType).ToArray();
        if (!o.Access.IsAdministrator && previous.Any(c => !o.Access.Allows(c.Value)))
            throw new AdministrationException("No puedes modificar un rol con privilegios superiores a los tuyos.");
        role.Name = input.Name;
        Ensure(input.Id is null ? await o.Roles.CreateAsync(role) : await o.Roles.UpdateAsync(role));
        foreach (var claim in previous) Ensure(await o.Roles.RemoveClaimAsync(role, claim));
        foreach (var permission in input.Permissions) Ensure(await o.Roles.AddClaimAsync(role, new Claim(PermissionNames.ClaimType, permission)));
        var memberIds = await o.Db.UserRoles.Where(r => r.RoleId == role.Id).Select(r => r.UserId).ToListAsync();
        foreach (var id in memberIds) Ensure(await o.Users.UpdateSecurityStampAsync((await o.Users.FindByIdAsync(id))!));
        Audit(o.Db, o.Access, input.Id is null ? "Role.Created" : "Role.Updated", "Role", role.Id, role.Name!,
            new { Name = beforeName, Permissions = previous.Select(c => c.Value) }, new { role.Name, input.Permissions });
        return true;
    });

    public Task DeleteRoleAsync(ActorSession actor, string id, string stamp) => RunAsync(actor, PermissionNames.RolesManage, true, async o =>
    {
        var role = await o.Roles.FindByIdAsync(id) ?? throw new AdministrationException("El rol ya no existe.");
        CheckStamp(role.ConcurrencyStamp, stamp);
        if (RoleNames.SystemRoles.Contains(role.Name)) throw new AdministrationException("No se pueden eliminar roles del sistema.");
        if (await o.Db.UserRoles.AnyAsync(r => r.RoleId == id)) throw new AdministrationException("Retira las asignaciones antes de eliminar este rol.");
        var claims = (await o.Roles.GetClaimsAsync(role)).Where(c => c.Type == PermissionNames.ClaimType).Select(c => c.Value).ToArray();
        if (!o.Access.IsAdministrator && claims.Any(p => !o.Access.Allows(p)))
            throw new AdministrationException("No puedes eliminar un rol con privilegios superiores a los tuyos.");
        Audit(o.Db, o.Access, "Role.Deleted", "Role", role.Id, role.Name!, new { role.Name, Permissions = claims });
        Ensure(await o.Roles.DeleteAsync(role)); return true;
    });

    public Task<PageResult<AuditRow>> GetAuditAsync(ActorSession actor, AuditQuery input) => RunAsync(actor, PermissionNames.AuditRead, false, async o =>
    {
        if (input.From > input.ToExclusive) throw new AdministrationException("Revisa el intervalo de fechas.");
        var q = o.Db.AuditEntries.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(input.Actor)) { var term = input.Actor.Trim().ToUpperInvariant(); q = q.Where(x => x.ActorName.ToUpper().Contains(term)); }
        if (!string.IsNullOrWhiteSpace(input.Operation)) { var term = input.Operation.Trim(); q = q.Where(x => x.Operation.Contains(term)); }
        if (!string.IsNullOrWhiteSpace(input.Target)) { var term = input.Target.Trim().ToUpperInvariant(); q = q.Where(x => x.TargetName.ToUpper().Contains(term) || x.TargetId.ToUpper().Contains(term)); }
        if (input.From.HasValue) q = q.Where(x => x.OccurredAtUtc >= input.From.Value);
        if (input.ToExclusive.HasValue) q = q.Where(x => x.OccurredAtUtc < input.ToExclusive.Value);
        var total = await q.CountAsync(); const int size = 20;
        var page = Math.Clamp(input.Page, 1, Math.Max(1, (total + size - 1) / size));
        var items = await q.OrderByDescending(x => x.Id).Skip((page - 1) * size).Take(size)
            .Select(x => new AuditRow(x.Id, x.OccurredAtUtc, x.ActorName, x.Operation, x.TargetType,
                x.TargetId, x.TargetName, x.BeforeJson, x.AfterJson)).ToListAsync();
        return new PageResult<AuditRow>(items, total, page, size);
    });
}
