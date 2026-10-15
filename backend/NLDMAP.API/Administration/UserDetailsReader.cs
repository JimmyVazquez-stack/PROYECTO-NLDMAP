using Microsoft.EntityFrameworkCore;
using NLDMAP.Application.Security;
using NLDMAP.Application.Users.Administration;
using NLDMAP.Infrastructure.Persistence;

namespace NLDMAP.API.Administration;

public sealed class UserDetailsReader(ApplicationDbContext database)
{
    public async Task<UserDetailsResponse?> ReadAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var user = await database.Users.AsNoTracking()
            .Where(account => account.Id == userId)
            .Select(account => new
            {
                account.Id,
                account.FirstName,
                account.LastName,
                Email = account.Email ?? string.Empty,
                account.IsActive,
                account.EmailConfirmed,
                account.TwoFactorEnabled,
                IsLockedOut = account.LockoutEnabled
                    && account.LockoutEnd != null && account.LockoutEnd > now,
                account.LockoutEnd,
                account.AccessFailedCount,
                Version = account.ConcurrencyStamp ?? string.Empty
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return null;
        }

        var assignedRoles = await (
            from membership in database.UserRoles.AsNoTracking()
            join role in database.Roles.AsNoTracking()
                on membership.RoleId equals role.Id
            where membership.UserId == userId && role.Name != null
            select role.Name!)
            .Distinct()
            .OrderBy(name => name)
            .ToArrayAsync(cancellationToken);

        return new UserDetailsResponse(
            user.Id, user.FirstName, user.LastName, user.Email,
            assignedRoles.Where(RoleNames.IsAssignable).ToArray(),
            assignedRoles.Where(role => !RoleNames.IsAssignable(role)).ToArray(),
            user.EmailConfirmed, user.TwoFactorEnabled, user.IsLockedOut,
            user.LockoutEnd, user.AccessFailedCount, user.Version, user.IsActive);
    }
}
