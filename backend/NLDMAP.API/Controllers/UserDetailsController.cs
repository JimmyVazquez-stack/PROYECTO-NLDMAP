using System.Security.Claims;
using NLDMAP.API.Administration;
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
using NLDMAP.Infrastructure.Persistence.Auditing;

namespace NLDMAP.API.Controllers;

[ApiController]
[Route("api/admin/users/{userId}")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class UserDetailsController(
    ApplicationDbContext database,
    UserDetailsReader reader,
    UserManager<ApplicationUser> users,
    PermissionAccessService permissionAccess,
    IAntiforgery antiforgery,
    ILogger<UserDetailsController> logger) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionNames.UsersRead)]
    public async Task<IActionResult> GetDetails(
        string userId,
        CancellationToken cancellationToken)
    {
        var details = await reader.ReadAsync(userId, cancellationToken);
        return details is null
            ? NotFound(new { message = "La cuenta no existe." })
            : Ok(details);
    }

    [HttpPut]
    [Consumes("application/json")]
    [Authorize(Policy = PermissionNames.UsersUpdate)]
    public async Task<IActionResult> UpdateProfile(
        string userId,
        [FromBody] UpdateUserProfileRequest request,
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

        // Share the administration lock so account writes and privilege changes are ordered.
        await using var transaction = await RoleAdministrationLock.AcquireAsync(
            database, cancellationToken);

        if (!await permissionAccess.HasPermissionAsync(
            actorId, PermissionNames.UsersUpdate, cancellationToken))
        {
            return Forbid();
        }

        var user = await users.FindByIdAsync(userId);

        if (user is null)
        {
            return NotFound(new { message = "La cuenta no existe." });
        }

        if (!string.Equals(
            user.ConcurrencyStamp, request.ExpectedVersion, StringComparison.Ordinal))
        {
            return Conflict(new
            {
                message = "La cuenta cambió. Vuelve a cargar sus datos antes de guardar."
            });
        }

        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        var changed = user.FirstName != firstName || user.LastName != lastName;

        if (changed)
        {
            var actor = await database.Users.AsNoTracking()
                .Where(account => account.Id == actorId)
                .Select(account => new { account.FirstName, account.LastName })
                .SingleOrDefaultAsync(cancellationToken);

            if (actor is null)
            {
                return Unauthorized();
            }

            var actorName = (actor.FirstName + " " + actor.LastName).Trim();
            var entry = new UserProfileAuditEntry
            {
                UserId = user.Id,
                ActorUserId = actorId,
                ActorDisplayName = string.IsNullOrEmpty(actorName) ? "Cuenta administrativa" : actorName,
                OccurredAtUtc = DateTimeOffset.UtcNow,
                PreviousFirstName = user.FirstName,
                PreviousLastName = user.LastName,
                NewFirstName = firstName,
                NewLastName = lastName
            };

            // Map only the editable fields. Identity email, roles and credentials are untouched.
            user.FirstName = firstName;
            user.LastName = lastName;
            var result = await users.UpdateAsync(user);

            if (!result.Succeeded)
            {
                logger.LogWarning(
                    "Profile update failed. ActorId={ActorId} TargetUserId={TargetUserId} ErrorCodes={ErrorCodes}",
                    actorId, userId, string.Join(",", result.Errors.Select(error => error.Code)));

                return Conflict(new
                {
                    message = "No fue posible guardar los datos. Vuelve a cargar la cuenta."
                });
            }

            // The Identity update and this audit entry commit or roll back together.
            database.UserProfileAuditEntries.Add(entry);
            await database.SaveChangesAsync(cancellationToken);
        }

        var details = await reader.ReadAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("La cuenta dejó de estar disponible.");

        await transaction.CommitAsync(cancellationToken);

        if (changed)
        {
            logger.LogInformation(
                "Profile updated. ActorId={ActorId} TargetUserId={TargetUserId}",
                actorId, userId);
        }

        return Ok(details);
    }

    [HttpGet("history")]
    [Authorize(Policy = PermissionNames.UsersRead)]
    public async Task<IActionResult> GetHistory(
        string userId,
        [FromQuery] UserHistoryRequest request,
        CancellationToken cancellationToken)
    {
        if (!await database.Users.AsNoTracking()
            .AnyAsync(account => account.Id == userId, cancellationToken))
        {
            return NotFound(new { message = "La cuenta no existe." });
        }

        var query = database.UserProfileAuditEntries.AsNoTracking()
            .Where(entry => entry.UserId == userId);
        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)(((long)totalCount + request.PageSize - 1) / request.PageSize);
        var offset = ((long)request.PageNumber - 1) * request.PageSize;
        UserProfileChangeResponse[] items = [];

        if (offset < totalCount)
        {
            items = await query.OrderByDescending(entry => entry.Id)
                .Skip((int)offset)
                .Take(request.PageSize)
                .Select(entry => new UserProfileChangeResponse(
                    entry.Id,
                    entry.UserId,
                    entry.ActorUserId,
                    entry.ActorDisplayName,
                    entry.OccurredAtUtc,
                    entry.PreviousFirstName,
                    entry.PreviousLastName,
                    entry.NewFirstName,
                    entry.NewLastName))
                .ToArrayAsync(cancellationToken);
        }

        return Ok(new UserProfileHistoryResponse(
            items, request.PageNumber, request.PageSize, totalCount, totalPages));
    }

}
