using Microsoft.Extensions.DependencyInjection;
using NLDMAP.Application.UseCases;
using NLDMAP.Application.UseCases.Auth;

namespace NLDMAP.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            //Registrar orquestadores
            services.AddScoped<AuthenticateExternalUserUseCase>();

            services.AddScoped<CreateReportUseCase>();
                
            return services;
        }
    }
}