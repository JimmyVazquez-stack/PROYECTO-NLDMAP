using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLDMAP.Infrastructure.Persistence;

namespace NLDMAP.API.Authorization;

public sealed class AccountCookieValidator(
    ApplicationDbContext database, IOptions<IdentityOptions> identityOptions)
{
    public async Task ValidateAsync(CookieValidatePrincipalContext context)
    {
        var claims = identityOptions.Value.ClaimsIdentity;
        var userId = context.Principal?.FindFirst(claims.UserIdClaimType)?.Value;
        var cookieStamp = context.Principal?.FindFirst(claims.SecurityStampClaimType)?.Value;
        var account = await database.Users.AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new { user.IsActive, user.SecurityStamp })
            .SingleOrDefaultAsync(context.HttpContext.RequestAborted);

        if (account is null || !account.IsActive || string.IsNullOrEmpty(cookieStamp)
            || !string.Equals(account.SecurityStamp, cookieStamp, StringComparison.Ordinal))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return;
        }

        // Preserve Identity's normal renewal and principal regeneration behavior.
        await SecurityStampValidator.ValidatePrincipalAsync(context);
    }
}
