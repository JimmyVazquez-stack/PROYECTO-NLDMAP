using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLDMAP.API.Authorization;
using NLDMAP.Application.Security;
using NLDMAP.Application.Users.Administration;
using NLDMAP.Infrastructure.Identity;
using NLDMAP.Infrastructure.Persistence;

namespace NLDMAP.API.Controllers;

[ApiController]
[Route("api/admin/users/{userId}/roles")]
[Authorize(Policy = PermissionNames.UsersAssignRoles)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class UserRolesController(
    ApplicationDbContext database,
    UserManager<ApplicationUser> users,
    RoleManager<IdentityRole> roles,
    PermissionAccessService permissionAccess,
    IAntiforgery antiforgery,
    ILogger<UserRolesController> logger) : ControllerBase
{
    [HttpPut]
    [Consumes("application/json")]
    public Task<IActionResult> Assign(
        string userId,
        [FromBody] ChangeUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        return ChangeAsync(userId, request, assign: true, cancellationToken);
    }

    [HttpDelete]
    [Consumes("application/json")]
    public Task<IActionResult> Remove(
        string userId,
        [FromBody] ChangeUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        return ChangeAsync(userId, request, assign: false, cancellationToken);
    }

    private async Task<IActionResult> ChangeAsync(
        string userId,
        ChangeUserRoleRequest request,
        bool assign,
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

        // Serialize membership writes and recheck authorization after obtaining the lock.
        await using var transaction = await RoleAdministrationLock.AcquireAsync(
            database, cancellationToken);

        if (!await permissionAccess.HasPermissionAsync(
            actorId, PermissionNames.UsersAssignRoles, cancellationToken))
        {
            return Forbid();
        }

        if (string.Equals(actorId, userId, StringComparison.Ordinal))
        {
            return Conflict(new
            {
                message = "No puedes modificar tus propios roles."
            });
        }

        var user = await users.FindByIdAsync(userId);

        if (user is null)
        {
            return NotFound(new { message = "La cuenta no existe." });
        }

        var role = await roles.FindByNameAsync(roleName);

        if (role is null || !string.Equals(role.Name, roleName, StringComparison.Ordinal))
        {
            return Conflict(new
            {
                message = "El catálogo de roles no está preparado. Ejecuta la semilla."
            });
        }

        var isMember = await users.IsInRoleAsync(user, roleName);

        // PUT and DELETE are idempotent: repeating a completed change is successful.
        if (isMember == assign)
        {
            await transaction.CommitAsync(cancellationToken);
            return NoContent();
        }

        if (!assign && roleName == RoleNames.SystemAdministrator && user.IsActive)
        {
            var administratorCount = await (
                from membership in database.UserRoles.AsNoTracking()
                join account in database.Users.AsNoTracking() on membership.UserId equals account.Id
                where membership.RoleId == role.Id && account.IsActive
                select membership.UserId).CountAsync(cancellationToken);

            if (administratorCount <= 1)
            {
                return Conflict(new
                {
                    message = "Debe conservarse al menos un administrador del sistema activo."
                });
            }
        }

        var result = assign
            ? await users.AddToRoleAsync(user, roleName)
            : await users.RemoveFromRoleAsync(user, roleName);

        if (!result.Succeeded)
        {
            logger.LogWarning(
                "Role change failed. ActorId={ActorId} TargetUserId={TargetUserId} " +
                "Role={Role} ErrorCodes={ErrorCodes}",
                actorId, userId, roleName,
                string.Join(",", result.Errors.Select(error => error.Code)));

            return Conflict(new
            {
                message = "No fue posible actualizar los roles. Actualiza los datos e inténtalo de nuevo."
            });
        }

        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Role membership changed. ActorId={ActorId} TargetUserId={TargetUserId} " +
            "Role={Role} Operation={Operation}",
            actorId, userId, roleName, assign ? "Assign" : "Remove");

        return NoContent();
    }
}
