using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection; 
using Neo4j.Driver;


namespace NLDMAP.Infrastructure.Graph;

public static class Neo4JRegistration 
{
    public static IServiceCollection AddNeo4J(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Agregar neo4j driver como un servicio singleton
        services.AddSingleton<IDriver>(_ => {
            var uri = configuration["Neo4j:Uri"];
            var username = configuration["Neo4j:Username"];
            var password = configuration["Neo4j:Password"];

            if (string.IsNullOrWhiteSpace(uri))
                throw new InvalidOperationException("Falta configurar Neo4j:Uri.");

            if (string.IsNullOrWhiteSpace(username))
                throw new InvalidOperationException("Falta configurar Neo4j:Username.");

            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Falta configurar Neo4j:Password.");

            return GraphDatabase.Driver(uri, AuthTokens.Basic(username, password));
        });
        return services;
    }
}
