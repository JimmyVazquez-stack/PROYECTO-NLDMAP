using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLDMAP.Application.Security;
using NLDMAP.Application.Users.Administration;
using NLDMAP.Infrastructure.Persistence;

namespace NLDMAP.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = PermissionNames.UsersRead)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class UsersController(ApplicationDbContext database) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<UserListResponse>> GetList(
        [FromQuery] UserListRequest request,
        CancellationToken cancellationToken)
    {
        var query = database.Users.AsNoTracking();
        var search = request.Search?.Trim();

        if (!string.IsNullOrEmpty(search))
        {
            // Treat user input as literal text, including LIKE wildcard characters.
            var pattern = $"%{EscapeLikePattern(search)}%";

            query = query.Where(user =>
                EF.Functions.ILike(user.Email ?? string.Empty, pattern, "\\")
                || EF.Functions.ILike(user.FirstName, pattern, "\\")
                || EF.Functions.ILike(user.LastName, pattern, "\\"));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)(((long)totalCount + request.PageSize - 1) / request.PageSize);
        var offset = ((long)request.PageNumber - 1) * request.PageSize;

        // Handle out-of-range pages before converting the offset to an EF Skip argument.
        if (offset >= totalCount)
        {
            return Ok(new UserListResponse(
                [], request.PageNumber, request.PageSize, totalCount, totalPages));
        }

        var page = await query
            .OrderBy(user => user.Email)
            .ThenBy(user => user.Id)
            .Skip((int)offset)
            .Take(request.PageSize)
            .Select(user => new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                Email = user.Email ?? string.Empty
            })
            .ToListAsync(cancellationToken);

        var userIds = page.Select(user => user.Id).ToArray();

        // Load memberships for the entire page in one query.
        var memberships = await (
            from membership in database.UserRoles.AsNoTracking()
            join role in database.Roles.AsNoTracking()
                on membership.RoleId equals role.Id
            where userIds.Contains(membership.UserId) && role.Name != null
            select new
            {
                membership.UserId,
                RoleName = role.Name!
            })
            .ToListAsync(cancellationToken);

        var rolesByUser = memberships.ToLookup(
            membership => membership.UserId,
            membership => membership.RoleName);

        var items = page.Select(user =>
        {
            var assignedRoles = rolesByUser[user.Id]
                .Distinct(StringComparer.Ordinal)
                .OrderBy(role => role, StringComparer.Ordinal)
                .ToArray();

            return new UserListItemResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                assignedRoles.Where(RoleNames.IsAssignable).ToArray(),
                assignedRoles.Where(role => !RoleNames.IsAssignable(role)).ToArray());
        }).ToArray();

        return Ok(new UserListResponse(
            items, request.PageNumber, request.PageSize, totalCount, totalPages));
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }
}

