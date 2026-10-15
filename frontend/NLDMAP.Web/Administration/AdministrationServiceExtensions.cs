using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NLDMAP.Web.Authentication;
using NLDMAP.Web.Administration.Caching;

namespace NLDMAP.Web.Administration;

public static class AdministrationServiceExtensions
{
    public static IServiceCollection AddAdministration(
        this IServiceCollection services)
    {
        services.AddAuthorizationCore(options =>
        {
            foreach (var permission in AdminPermissions.All)
            {
                options.AddPolicy(permission, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(AdminPermissions.ClaimType, permission);
                });
            }
        });

        services.AddCascadingAuthenticationState();
        services.AddScoped<AdminApiClient>();
        services.AddScoped<AdminCacheSession>();
        services.AddScoped<AdminDataChangeNotifier>();
        services.AddScoped<AdminDataStore>();
        services.AddScoped<CookieAuthStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(provider =>
            provider.GetRequiredService<CookieAuthStateProvider>());

        return services;
    }
}
