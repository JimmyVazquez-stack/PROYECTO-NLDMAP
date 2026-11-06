using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NLDMAP.Application.Administration;

namespace NLDMAP.Server.Authentication;

public sealed class CurrentActor(AuthenticationStateProvider authentication, IOptions<IdentityOptions> options) : ICurrentActor
{
    public async Task<ActorSession> GetAsync()
    {
        var principal = (await authentication.GetAuthenticationStateAsync()).User;
        if (principal.Identity?.IsAuthenticated != true) throw new AdministrationException("Inicia sesión para continuar.");
        return FromPrincipal(principal, options.Value);
    }
    public static ActorSession FromPrincipal(ClaimsPrincipal principal, IdentityOptions options) =>
        new(principal.FindFirstValue(options.ClaimsIdentity.UserIdClaimType) ?? "",
            principal.FindFirstValue(options.ClaimsIdentity.SecurityStampClaimType) ?? "");
}
