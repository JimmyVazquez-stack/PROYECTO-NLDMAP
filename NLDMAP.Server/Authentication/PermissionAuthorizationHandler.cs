using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Security;

namespace NLDMAP.Server.Authentication;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;
public sealed class PermissionAuthorizationHandler(IAdministrationStore store, IOptions<IdentityOptions> options)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true) return;
        try
        {
            var access = await store.GetAccessAsync(CurrentActor.FromPrincipal(context.User, options.Value));
            if (requirement.Permission == PermissionNames.Access || access.Allows(requirement.Permission)) context.Succeed(requirement);
        }
        catch (AdministrationException) { /* Denegar una sesión revocada o una cuenta sin permisos. */ }
    }
}
