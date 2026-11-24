using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NLDMAP.Application.Reports;
using NLDMAP.Application.Security;

namespace NLDMAP.Server.Authentication;

public sealed class ReportCreationRequirement : IAuthorizationRequirement
{
    public const string Policy = PermissionNames.ReportsCreate;
}

public sealed class ReportCreationAuthorizationHandler(
    ReportAccess access, IOptions<IdentityOptions> identity)
    : AuthorizationHandler<ReportCreationRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, ReportCreationRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return;

        var actor = CurrentActor.FromPrincipal(context.User, identity.Value);
        if (await access.CanCreateAsync(actor))
            context.Succeed(requirement);
    }
}
