using System.Data;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Security;
using NLDMAP.Domain.Auditing;
using NLDMAP.Infrastructure.Identity;
using NLDMAP.Infrastructure.Persistence;

namespace NLDMAP.Infrastructure.Administration;

public sealed partial class AdministrationStore(IServiceScopeFactory scopeFactory, AccountEmailSender emailSender)
    : IAdministrationStore
{
    private sealed record Operation(ApplicationDbContext Db, UserManager<ApplicationUser> Users,
        RoleManager<IdentityRole> Roles, AdminAccess Access);

    private async Task<T> RunAsync<T>(ActorSession actor, string permission, bool write, Func<Operation, Task<T>> action)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await using var tx = write ? await db.Database.BeginTransactionAsync(db.Database.IsNpgsql() ? IsolationLevel.ReadCommitted : IsolationLevel.Serializable) : null;
        // Serializa cambios administrativos para que el conteo del último administrador
        // y los privilegios del actor no cambien entre la comprobación y la escritura.
        if (write && db.Database.IsNpgsql())
            await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(724910031)");
        try
        {
            var access = await AccessAsync(db, actor);
            AdministrationRules.RequirePermission(access, permission);
            var result = await action(new(db, users, roles, access));
            if (write) { await db.SaveChangesAsync(); await tx!.CommitAsync(); }
            return result;
        }
        catch (DbUpdateConcurrencyException)
        { throw new AdministrationException("Los datos cambiaron. Actualiza la página y vuelve a intentarlo."); }
        catch (DbUpdateException)
        { throw new AdministrationException("No se pudo guardar. Revisa si los datos ya existen y actualiza la página."); }
    }

    internal static async Task<AdminAccess> AccessAsync(ApplicationDbContext db, ActorSession actor)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == actor.UserId);
        if (user is null || !user.IsActive || string.IsNullOrEmpty(actor.SecurityStamp) || user.SecurityStamp != actor.SecurityStamp)
            throw new AdministrationException("La sesión cambió o la cuenta está desactivada. Inicia sesión nuevamente.");
        return await AccessForUserAsync(db, user);
    }

    internal static async Task<AdminAccess> AccessForUserAsync(ApplicationDbContext db, ApplicationUser user)
    {
        var roleIds = db.UserRoles.Where(r => r.UserId == user.Id).Select(r => r.RoleId);
        var administrator = await db.Roles.AnyAsync(r => roleIds.Contains(r.Id) && r.Name == RoleNames.Administrator);
        var permissions = administrator ? PermissionNames.Catalog.Keys.ToArray() :
            (await db.RoleClaims.Where(c => roleIds.Contains(c.RoleId) && c.ClaimType == PermissionNames.ClaimType)
                .Select(c => c.ClaimValue!).Distinct().ToArrayAsync()).Where(PermissionNames.Catalog.ContainsKey).ToArray();
        return new(user.Id, user.Email ?? user.UserName ?? user.Id, administrator, permissions);
    }

    public Task<AdminAccess> GetAccessAsync(ActorSession actor) => RunAsync(actor, PermissionNames.Access, false, o => Task.FromResult(o.Access));

    internal static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded) throw new AdministrationException(string.Join(" ", result.Errors.Select(e => e.Description)));
    }
    private static void CheckStamp(string? actual, string? expected)
    {
        if (string.IsNullOrEmpty(expected) || actual != expected)
            throw new AdministrationException("Otra operación modificó este registro. Actualiza antes de guardar.");
    }
    private static async Task<ApplicationUser> UserAsync(Operation o, string id, string stamp)
    {
        var user = await o.Users.FindByIdAsync(id) ?? throw new AdministrationException("La cuenta ya no existe.");
        CheckStamp(user.ConcurrencyStamp, stamp); return user;
    }
    internal static void Audit(ApplicationDbContext db, AdminAccess actor, string operation, string type,
        string id, string name, object? before = null, object? after = null)
        => db.AuditEntries.Add(new AuditEntry { OccurredAtUtc = DateTimeOffset.UtcNow,
            ActorId = actor.UserId, ActorName = actor.Name, Operation = operation, TargetType = type,
            TargetId = id, TargetName = name, BeforeJson = JsonSerializer.Serialize(before ?? new { }),
            AfterJson = JsonSerializer.Serialize(after ?? new { }) });

    public Task<PageResult<UserRow>> GetUsersAsync(ActorSession actor, UserQuery input) =>
        RunAsync(actor, PermissionNames.UsersRead, false, async o =>
        {
            var query = o.Db.Users.AsNoTracking();
            var search = (input.Search ?? "").Trim();
            if (search.Length > 100) search = search[..100];
            if (search.Length > 0) { var normalized = search.ToUpperInvariant();
                query = query.Where(u => (u.NormalizedEmail ?? "").Contains(normalized) || u.DisplayName.ToUpper().Contains(normalized)); }
            if (input.IsActive.HasValue) query = query.Where(u => u.IsActive == input.IsActive.Value);
            if (!string.IsNullOrEmpty(input.RoleId)) query = query.Where(u => o.Db.UserRoles.Any(r => r.UserId == u.Id && r.RoleId == input.RoleId));
            var count = await query.CountAsync(); const int size = 20;
            var page = Math.Clamp(input.Page, 1, Math.Max(1, (count + size - 1) / size));
            var users = await query.OrderBy(u => u.Email).ThenBy(u => u.Id).Skip((page - 1) * size).Take(size).ToListAsync();
            var ids = users.Select(u => u.Id).ToArray();
            var names = await (from ur in o.Db.UserRoles join r in o.Db.Roles on ur.RoleId equals r.Id
                where ids.Contains(ur.UserId) select new { ur.UserId, r.Name }).ToListAsync();
            return new PageResult<UserRow>(users.Select(u => new UserRow(u.Id, u.Email ?? "", u.DisplayName, u.IsActive,
                names.Where(r => r.UserId == u.Id).Select(r => r.Name!).Order().ToArray())).ToArray(), count, page, size);
        });

    public Task<UserDetail> GetUserAsync(ActorSession actor, string id) => RunAsync(actor, PermissionNames.UsersRead, false, async o =>
    {
        var u = await o.Users.FindByIdAsync(id) ?? throw new AdministrationException("No se encontró la cuenta.");
        return new UserDetail(u.Id, u.Email ?? "", u.DisplayName, u.PhoneNumber, u.IsActive, u.ConcurrencyStamp!,
            await o.Db.UserRoles.Where(r => r.UserId == id).Select(r => r.RoleId).ToArrayAsync());
    });

    public Task UpdateUserAsync(ActorSession actor, UserProfileInput input) => RunAsync(actor, PermissionNames.UsersEdit, true, async o =>
    {
        var u = await UserAsync(o, input.UserId, input.ConcurrencyStamp);
        var before = new { u.DisplayName, u.PhoneNumber };
        u.DisplayName = input.DisplayName; u.PhoneNumber = input.PhoneNumber?.Trim();
        Ensure(await o.Users.UpdateAsync(u));
        Audit(o.Db, o.Access, "User.ProfileUpdated", "User", u.Id, u.Email!, before, new { u.DisplayName, u.PhoneNumber });
        return true;
    });

    private static async Task ProtectLastAdminAsync(Operation o, ApplicationUser user, bool removesAdministrator)
    {
        if (!user.IsActive || !removesAdministrator || !await o.Users.IsInRoleAsync(user, RoleNames.Administrator)) return;
        var count = await (from u in o.Db.Users join ur in o.Db.UserRoles on u.Id equals ur.UserId
            join r in o.Db.Roles on ur.RoleId equals r.Id where u.IsActive && r.Name == RoleNames.Administrator select u.Id).Distinct().CountAsync();
        AdministrationRules.PreserveActiveAdministrator(count);
    }

    public Task SetUserStatusAsync(ActorSession actor, ChangeStatusInput input) => RunAsync(actor, PermissionNames.UsersStatus, true, async o =>
    {
        var u = await UserAsync(o, input.UserId, input.ConcurrencyStamp);
        if (!input.IsActive) await ProtectLastAdminAsync(o, u, true);
        if (u.Id == actor.UserId && !input.IsActive) throw new AdministrationException("No puedes desactivar tu propia cuenta.");
        if (!o.Access.IsAdministrator && await o.Users.IsInRoleAsync(u, RoleNames.Administrator))
            throw new AdministrationException("Solo otro administrador puede cambiar el estado de esta cuenta.");
        var before = new { u.IsActive }; u.IsActive = input.IsActive;
        Ensure(await o.Users.UpdateSecurityStampAsync(u));
        Audit(o.Db, o.Access, "User.StatusChanged", "User", u.Id, u.Email!, before, new { u.IsActive });
        return true;
    });

    internal static async Task<IdentityRole[]> ValidateGrantAsync(ApplicationDbContext db, AdminAccess access, string[] roleIds)
    {
        var ids = roleIds.Distinct().ToArray();
        var roles = await db.Roles.Where(r => ids.Contains(r.Id)).ToArrayAsync();
        if (roles.Length != ids.Length) throw new AdministrationException("Uno de los roles ya no existe.");
        if (!access.IsAdministrator)
        {
            var granted = await db.RoleClaims.Where(c => ids.Contains(c.RoleId) && c.ClaimType == PermissionNames.ClaimType)
                .Select(c => c.ClaimValue!).ToArrayAsync();
            if (roles.Any(r => r.Name == RoleNames.Administrator) || granted.Any(p => !access.Allows(p)))
                throw new AdministrationException("No puedes otorgar privilegios superiores a los tuyos.");
        }
        return roles;
    }

    public Task SetUserRolesAsync(ActorSession actor, ChangeRolesInput input) => RunAsync(actor, PermissionNames.RolesAssign, true, async o =>
    {
        var u = await UserAsync(o, input.UserId, input.ConcurrencyStamp);
        var roles = await ValidateGrantAsync(o.Db, o.Access, input.RoleIds);
        var names = roles.Select(r => r.Name!).ToArray();
        var previous = (await o.Users.GetRolesAsync(u)).ToArray();
        if (!o.Access.IsAdministrator && previous.Contains(RoleNames.Administrator))
            throw new AdministrationException("Solo un administrador puede modificar esta asignación.");
        await ProtectLastAdminAsync(o, u, !names.Contains(RoleNames.Administrator));
        Ensure(await o.Users.RemoveFromRolesAsync(u, previous.Except(names)));
        Ensure(await o.Users.AddToRolesAsync(u, names.Except(previous)));
        Ensure(await o.Users.UpdateSecurityStampAsync(u));
        Audit(o.Db, o.Access, "User.RolesChanged", "User", u.Id, u.Email!, new { Roles = previous }, new { Roles = names });
        return true;
    });
}
