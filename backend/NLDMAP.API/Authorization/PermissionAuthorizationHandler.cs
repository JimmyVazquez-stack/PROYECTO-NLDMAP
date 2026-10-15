using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace NLDMAP.API.Authorization;

public sealed class PermissionAuthorizationHandler(
    PermissionAccessService permissionAccess)
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

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var cancellationToken =
            (context.Resource as HttpContext)?.RequestAborted
            ?? CancellationToken.None;

        if (await permissionAccess.HasPermissionAsync(
                userId, requirement.Permission, cancellationToken))
        {
            context.Succeed(requirement);
        }
    }
}
