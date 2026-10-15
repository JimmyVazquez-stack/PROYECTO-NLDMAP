using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLDMAP.API.Administration;
using NLDMAP.API.Authorization;
using NLDMAP.Application.Security;
using NLDMAP.Application.Users.Administration;
using NLDMAP.Infrastructure.Identity;
using NLDMAP.Infrastructure.Persistence;
using NLDMAP.Infrastructure.Persistence.Auditing;

namespace NLDMAP.API.Controllers;

[ApiController]
[Route("api/admin/users/{userId}/status")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class UserAccountStatusController(
    ApplicationDbContext database, UserManager<ApplicationUser> users,
    UserDetailsReader reader, PermissionAccessService permissionAccess,
    IAntiforgery antiforgery, ILogger<UserAccountStatusController> logger) : ControllerBase
{
    [HttpPut]
    [Consumes("application/json")]
    [Authorize(Policy = PermissionNames.UsersUpdate)]
    public async Task<IActionResult> Update(
        string userId, [FromBody] UpdateUserAccountStatusRequest request,
        CancellationToken cancellationToken)
    {
        try { await antiforgery.ValidateRequestAsync(HttpContext); }
        catch (AntiforgeryValidationException)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "El token de protección de la solicitud falta o no es válido.");
        }

        var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(actorId)) { return Unauthorized(); }
        var isActive = request.IsActive!.Value;
        if (!isActive && actorId == userId)
        {
            return Conflict(new { message = "No puedes desactivar tu propia cuenta." });
        }

        await using var transaction = await RoleAdministrationLock.AcquireAsync(database, cancellationToken);
        if (!await permissionAccess.HasPermissionAsync(actorId, PermissionNames.UsersUpdate, cancellationToken))
        {
            return Forbid();
        }
        var user = await users.FindByIdAsync(userId);
        if (user is null) { return NotFound(new { message = "La cuenta no existe." }); }
        if (!string.Equals(user.ConcurrencyStamp, request.ExpectedVersion, StringComparison.Ordinal))
        {
            return Conflict(new { message = "La cuenta cambió. Actualiza la ficha antes de cambiar su estado." });
        }

        var changed = user.IsActive != isActive;
        if (changed)
        {
            if (!isActive && await users.IsInRoleAsync(user, RoleNames.SystemAdministrator))
            {
                var activeAdministrators = await (
                    from membership in database.UserRoles.AsNoTracking()
                    join role in database.Roles.AsNoTracking() on membership.RoleId equals role.Id
                    join account in database.Users.AsNoTracking() on membership.UserId equals account.Id
                    where role.Name == RoleNames.SystemAdministrator && account.IsActive
                    select account.Id).Distinct().CountAsync(cancellationToken);
                if (activeAdministrators <= 1)
                {
                    return Conflict(new { message = "Debe conservarse al menos un administrador del sistema activo." });
                }
            }

            var actor = await database.Users.AsNoTracking().Where(account => account.Id == actorId)
                .Select(account => new { account.FirstName, account.LastName })
                .SingleOrDefaultAsync(cancellationToken);
            if (actor is null) { return Unauthorized(); }
            var actorName = (actor.FirstName + " " + actor.LastName).Trim();
            var entry = new UserAccountStatusAuditEntry
            {
                UserId = user.Id, ActorUserId = actorId,
                ActorDisplayName = string.IsNullOrEmpty(actorName) ? "Cuenta administrativa" : actorName,
                OccurredAtUtc = DateTimeOffset.UtcNow,
                PreviousIsActive = user.IsActive, NewIsActive = isActive, Reason = request.Reason.Trim()
            };
            user.IsActive = isActive;
            // Rotate on deactivation AND reactivation. Previously issued cookies never revive.
            var result = await users.UpdateSecurityStampAsync(user);
            if (!result.Succeeded)
            {
                logger.LogWarning("Account status update failed. ActorId={ActorId} TargetUserId={TargetUserId} ErrorCodes={ErrorCodes}",
                    actorId, userId, string.Join(",", result.Errors.Select(error => error.Code)));
                return Conflict(new { message = "No se pudo cambiar el estado. Actualiza la ficha e inténtalo de nuevo." });
            }
            database.UserAccountStatusAuditEntries.Add(entry);
            await database.SaveChangesAsync(cancellationToken);
        }

        var details = await reader.ReadAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("La cuenta dejó de estar disponible.");
        await transaction.CommitAsync(cancellationToken);
        if (changed)
        {
            logger.LogInformation("Account status changed. ActorId={ActorId} TargetUserId={TargetUserId} IsActive={IsActive}",
                actorId, userId, isActive);
        }
        return Ok(details);
    }

    [HttpGet("history")]
    [Authorize(Policy = PermissionNames.UsersRead)]
    public async Task<IActionResult> History(
        string userId, [FromQuery] UserHistoryRequest request, CancellationToken cancellationToken)
    {
        if (!await database.Users.AsNoTracking().AnyAsync(user => user.Id == userId, cancellationToken))
        {
            return NotFound(new { message = "La cuenta no existe." });
        }
        var query = database.UserAccountStatusAuditEntries.AsNoTracking().Where(entry => entry.UserId == userId);
        var total = await query.CountAsync(cancellationToken);
        var totalPages = (int)(((long)total + request.PageSize - 1) / request.PageSize);
        var offset = ((long)request.PageNumber - 1) * request.PageSize;
        UserAccountStatusChangeResponse[] items = [];
        if (offset < total)
        {
            items = await query.OrderByDescending(entry => entry.Id).Skip((int)offset).Take(request.PageSize)
                .Select(entry => new UserAccountStatusChangeResponse(entry.Id, entry.UserId, entry.ActorUserId,
                    entry.ActorDisplayName, entry.OccurredAtUtc, entry.PreviousIsActive, entry.NewIsActive, entry.Reason))
                .ToArrayAsync(cancellationToken);
        }
        return Ok(new UserAccountStatusHistoryResponse(items, request.PageNumber, request.PageSize, total, totalPages));
    }
}
