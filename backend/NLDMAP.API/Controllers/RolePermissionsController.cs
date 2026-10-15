using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NLDMAP.API.Authorization;
using NLDMAP.Application.Security;
using NLDMAP.Application.Users.Administration;
using NLDMAP.Infrastructure.Persistence;

namespace NLDMAP.API.Controllers;

[ApiController]
[Route("api/admin/roles/permissions")]
[Authorize(Policy = PermissionNames.RolesUpdatePermissions)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class RolePermissionsController(
    ApplicationDbContext database,
    RoleManager<IdentityRole> roles,
    PermissionAccessService permissionAccess,
    IAntiforgery antiforgery,
    ILogger<RolePermissionsController> logger) : ControllerBase
{
    [HttpPut]
    [Consumes("application/json")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "El token de protección de la solicitud falta o no es válido.");
        }

        var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(actorId))
        {
            return Unauthorized();
        }

        var roleName = request.RoleName.Trim();

        if (!RoleNames.IsAssignable(roleName))
        {
            ModelState.AddModelError(
                nameof(request.RoleName), "Selecciona un rol del catálogo vigente.");
            return ValidationProblem(ModelState);
        }

        // Null elements are possible in JSON arrays despite the nonnullable element type.
        if (request.Permissions is null
            || request.Permissions.Any(string.IsNullOrWhiteSpace))
        {
            ModelState.AddModelError(
                nameof(request.Permissions), "La lista contiene permisos vacíos o no válidos.");
            return ValidationProblem(ModelState);
        }

        var requested = request.Permissions.ToHashSet(StringComparer.Ordinal);

        if (requested.Count != request.Permissions.Length)
        {
            ModelState.AddModelError(
                nameof(request.Permissions), "La lista no debe contener permisos repetidos.");
        }

        if (requested.Any(permission => !RolePermissionDefaults.Allows(roleName, permission)))
        {
            ModelState.AddModelError(
                nameof(request.Permissions), "El rol no admite uno o más de los permisos indicados.");
        }

        // Preserve the technical baseline already established by IdentitySeeder.
        if (roleName == RoleNames.SystemAdministrator
            && !requested.SetEquals(PermissionNames.Technical))
        {
            ModelState.AddModelError(
                nameof(request.Permissions),
                "El administrador del sistema debe conservar sus ocho permisos técnicos.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        await using var transaction = await RoleAdministrationLock.AcquireAsync(
            database, cancellationToken);

        if (!await permissionAccess.HasPermissionAsync(
            actorId, PermissionNames.RolesUpdatePermissions, cancellationToken))
        {
            return Forbid();
        }

        var role = await roles.FindByNameAsync(roleName);

        if (role is null || !string.Equals(role.Name, roleName, StringComparison.Ordinal))
        {
            return Conflict(new
            {
                message = "El catálogo de roles no está preparado. Ejecuta la semilla."
            });
        }

        if (!string.Equals(
            role.ConcurrencyStamp, request.ExpectedVersion, StringComparison.Ordinal))
        {
            return Conflict(new
            {
                message = "La configuración cambió. Vuelve a cargar el rol antes de guardar."
            });
        }

        var claims = await roles.GetClaimsAsync(role);
        var permissionClaims = claims
            .Where(claim => claim.Type == PermissionNames.ClaimType)
            .ToArray();
        var current = permissionClaims
            .Select(claim => claim.Value)
            .ToHashSet(StringComparer.Ordinal);

        // A matching version and an unchanged set are a successful no-op.
        var changed = !current.SetEquals(requested)
            || permissionClaims.Length != requested.Count;

        if (changed)
        {
            // Preserve claims of other types and remove duplicate/out-of-bound permission claims.
            foreach (var claim in permissionClaims.DistinctBy(claim => claim.Value))
            {
                var result = await roles.RemoveClaimAsync(role, claim);

                if (!result.Succeeded)
                {
                    return SaveConflict(result, actorId, roleName);
                }
            }

            foreach (var permission in requested.OrderBy(value => value, StringComparer.Ordinal))
            {
                var result = await roles.AddClaimAsync(
                    role, new Claim(PermissionNames.ClaimType, permission));

                if (!result.Succeeded)
                {
                    return SaveConflict(result, actorId, roleName);
                }
            }
        }

        await transaction.CommitAsync(cancellationToken);

        if (changed)
        {
            logger.LogInformation(
                "Role permissions replaced. ActorId={ActorId} Role={Role} " +
                "Before={Before} After={After}",
                actorId, roleName,
                string.Join(",", current.OrderBy(value => value, StringComparer.Ordinal)),
                string.Join(",", requested.OrderBy(value => value, StringComparer.Ordinal)));
        }

        return Ok(new RoleListItemResponse(
            roleName,
            requested.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            RolePermissionDefaults.GetAllowedPermissions(roleName)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray(),
            role.ConcurrencyStamp ?? string.Empty));
    }

    private IActionResult SaveConflict(
        IdentityResult result,
        string actorId,
        string roleName)
    {
        logger.LogWarning(
            "Role permission update failed. ActorId={ActorId} Role={Role} ErrorCodes={ErrorCodes}",
            actorId, roleName, string.Join(",", result.Errors.Select(error => error.Code)));

        return Conflict(new
        {
            message = "No fue posible guardar los permisos. Vuelve a cargar el rol e inténtalo de nuevo."
        });
    }
}

