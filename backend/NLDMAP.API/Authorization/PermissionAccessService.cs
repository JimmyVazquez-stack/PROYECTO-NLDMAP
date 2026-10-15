using Microsoft.EntityFrameworkCore;
using NLDMAP.Application.Security;
using NLDMAP.Infrastructure.Persistence;

namespace NLDMAP.API.Authorization;

public sealed class PermissionAccessService(ApplicationDbContext database)
{
    public async Task<bool> HasPermissionAsync(
        string userId,
        string permission,
        CancellationToken cancellationToken = default)
    {
        var allowedRoles = RolePermissionDefaults.GetRolesForPermission(permission);

        if (allowedRoles.Length == 0)
        {
            return false;
        }

        return await QueryPermissionGrants(userId)
            .AnyAsync(
                grant => grant.Permission == permission
                    && allowedRoles.Contains(grant.RoleName),
                cancellationToken);
    }

    public async Task<string[]> GetPermissionsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var grants = await QueryPermissionGrants(userId)
            .ToArrayAsync(cancellationToken);

        return grants
            .Where(grant => RolePermissionDefaults.Allows(
                grant.RoleName, grant.Permission))
            .Select(grant => grant.Permission)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(permission => permission, StringComparer.Ordinal)
            .ToArray();
    }

    private IQueryable<PermissionGrant> QueryPermissionGrants(string userId)
    {
        // A stored claim must also respect the approved boundary for its actual role.
        return
            from membership in database.UserRoles.AsNoTracking()
            join account in database.Users.AsNoTracking()
                on membership.UserId equals account.Id
            join role in database.Roles.AsNoTracking()
                on membership.RoleId equals role.Id
            join claim in database.RoleClaims.AsNoTracking()
                on role.Id equals claim.RoleId
            where membership.UserId == userId
                && account.IsActive
                && role.Name != null
                && claim.ClaimType == PermissionNames.ClaimType
                && claim.ClaimValue != null
            select new PermissionGrant
            {
                RoleName = role.Name!,
                Permission = claim.ClaimValue!
            };
    }

    private sealed class PermissionGrant
    {
        public string RoleName { get; init; } = string.Empty;
        public string Permission { get; init; } = string.Empty;
    }
}
