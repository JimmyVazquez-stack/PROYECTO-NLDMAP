using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLDMAP.Infrastructure.Identity;
using NLDMAP.Infrastructure.Persistence;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Accounts;
using NLDMAP.Infrastructure.Administration;

namespace NLDMAP.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure( this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Falta configurar ConnectionString:DefaultConnection");
            }

            services.AddDbContextFactory<ApplicationDbContext>(options => { options.UseNpgsql(connectionString); });
            services.AddIdentityCore<ApplicationUser>(options => { options.User.RequireUniqueEmail = true; options.Password.RequiredLength = 12; })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager<ActiveSignInManager>()
            .AddDefaultTokenProviders();
            services.AddScoped<IdentitySeeder>();
            services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(2));
            services.AddScoped<IAdministrationStore, AdministrationStore>();
            services.AddScoped<IAccountLifecycle, AccountLifecycle>();
            services.AddScoped<AccountEmailSender>();

            return services;
        }
    }
}
