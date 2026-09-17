using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using NLDMAP.Domain.Interfaces;
using NLDMAP.Domain.Repositories;
using NLDMAP.Infrastructure.ExternalServices;
using NLDMAP.Infrastructure.Persistence;
using NLDMAP.Infrastructure.Repositories;
using Neo4j.Driver;
 


namespace NLDMAP.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            //modulo 1 : Identidad y seguridad 
            services.AddPersistenceInfrastructure(configuration);
            
            //modulo 2 : Documentos y caracteristicas 
            services.AddSingleton<IMongoClient>(sp =>
            {
                var connectionString = configuration.GetConnectionString("MongoConnection");
                return new MongoClient(connectionString);
            });
            
            //Modulo 3: Grafos
            services.AddSingleton<IDriver>(sp =>
            {
                var uri = configuration.GetConnectionString("Neo4jConnection");
                var user = configuration["Neo4jSettings:User"];
                var password = configuration["Neo4jSettings:Password"];

                return GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
            });
            services.AddScoped<IUserRepository, EfUserRepository>();
            services.AddScoped<IReportRepository, EfReportRepository>();
            services.AddScoped<IExternalAuthValidator, GoogleAuthValidator>();
        
            return services;
        }
    }
}