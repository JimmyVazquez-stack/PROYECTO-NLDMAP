using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLDMAP.Application.Security;
using NLDMAP.Application.Users.Administration;
using NLDMAP.Infrastructure.Persistence;

namespace NLDMAP.API.Controllers;

[ApiController]
[Route("api/admin/roles")]
[Authorize(Policy = PermissionNames.RolesRead)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class RolesController(ApplicationDbContext database) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<RoleListItemResponse[]>> GetList(
        CancellationToken cancellationToken)
    {
        // Read each role version and its claims from the same database snapshot.
        await using var transaction = await database.Database.BeginTransactionAsync(
            IsolationLevel.RepeatableRead, cancellationToken);

        var names = RoleNames.All.ToArray();
        var roles = await database.Roles.AsNoTracking()
            .Where(role => role.Name != null && names.Contains(role.Name))
            .Select(role => new { role.Id, Name = role.Name!, role.ConcurrencyStamp })
            .ToArrayAsync(cancellationToken);

        var roleIds = roles.Select(role => role.Id).ToArray();
        var claims = await database.RoleClaims.AsNoTracking()
            .Where(claim => roleIds.Contains(claim.RoleId)
                && claim.ClaimType == PermissionNames.ClaimType
                && claim.ClaimValue != null)
            .Select(claim => new { claim.RoleId, Permission = claim.ClaimValue! })
            .ToArrayAsync(cancellationToken);

        var claimsByRole = claims.ToLookup(
            claim => claim.RoleId, claim => claim.Permission);

        var items = roles.Select(role => new RoleListItemResponse(
            role.Name,
            claimsByRole[role.Id]
                .Where(permission => RolePermissionDefaults.Allows(role.Name, permission))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(permission => permission, StringComparer.Ordinal)
                .ToArray(),
            RolePermissionDefaults.GetAllowedPermissions(role.Name)
                .OrderBy(permission => permission, StringComparer.Ordinal)
                .ToArray(),
            role.ConcurrencyStamp ?? string.Empty))
            .OrderBy(role => Array.IndexOf(names, role.Name))
            .ToArray();

        await transaction.CommitAsync(cancellationToken);
        return Ok(items);
    }
}

