using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Security;

namespace NLDMAP.Server.Authentication;

public sealed record PermissionRequirement(string Permission)
    : IAuthorizationRequirement;

public sealed class PermissionAuthorizationHandler(
    AdministrationAccessCache accessCache,
    IOptions<IdentityOptions> options)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        try
        {
            var actor = CurrentActor.FromPrincipal(
                context.User,
                options.Value);

            var access =
                await accessCache.GetAsync(actor);

            if (requirement.Permission == PermissionNames.Access
                || access.Allows(requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
        catch (AdministrationException)
        {
            // Sesión revocada, cuenta inactiva
            // o acceso administrativo no válido.
        }
    }
}