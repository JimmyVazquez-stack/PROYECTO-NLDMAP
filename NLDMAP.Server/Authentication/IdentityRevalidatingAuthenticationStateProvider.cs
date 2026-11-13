using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NLDMAP.Infrastructure.Identity;

namespace NLDMAP.Server.Authentication;

public sealed class IdentityRevalidatingAuthenticationStateProvider(
    ILoggerFactory loggerFactory,
    IServiceScopeFactory scopeFactory,
    IOptions<IdentityOptions> identityOptions)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval =>
        TimeSpan.FromMinutes(1);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var scope = scopeFactory.CreateAsyncScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.GetUserAsync(
            authenticationState.User);

        if (user is null || !user.IsActive)
        {
            return false;
        }

        if (!userManager.SupportsUserSecurityStamp)
        {
            return true;
        }

        var claimType =
            identityOptions.Value.ClaimsIdentity.SecurityStampClaimType;

        var sessionStamp = authenticationState.User
            .FindFirst(claimType)?.Value;

        var currentStamp =
            await userManager.GetSecurityStampAsync(user);

        return !string.IsNullOrEmpty(sessionStamp)
            && sessionStamp == currentStamp;
    }
}
