using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NLDMAP.Application.Security;

namespace NLDMAP.API.Authorization;

public static class PermissionAuthorizationExtensions
{
    public static IServiceCollection AddPermissionAuthorization(
        this IServiceCollection services)
    {
        services.AddScoped<PermissionAccessService>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            foreach (var permission in PermissionNames.All)
            {
                options.AddPolicy(permission, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.AddRequirements(
                        new PermissionRequirement(permission));

                    if (PermissionNames.Functional.Contains(
                            permission, StringComparer.Ordinal))
                    {
                        // Role membership alone never grants access to domain resources.
                        policy.AddRequirements(
                            new PermissionScopeRequirement(permission));
                    }
                });
            }
        });

        return services;
    }
}
