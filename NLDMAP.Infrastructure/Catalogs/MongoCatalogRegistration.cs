using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

public static class MongoCatalogRegistration
{
    public static IServiceCollection AddMongoCatalogs(this IServiceCollection services, IConfiguration configuration)
    {
        // Se resuelve al abrir el catálogo. Identity y las migraciones SQL pueden
        // seguir ejecutándose antes de configurar o iniciar MongoDB.
        services.AddSingleton<IMongoClient>(_ =>
        {
            var connectionString = configuration["MongoDb:ConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new AdministrationException("Falta configurar MongoDb:ConnectionString en el servidor.");
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
            settings.ConnectTimeout = TimeSpan.FromSeconds(5);
            settings.SocketTimeout = TimeSpan.FromSeconds(15);
            return new MongoClient(settings);
        });
        services.AddSingleton<IMongoDatabase>(provider =>
        {
            var database = configuration["MongoDb:Database"];
            if (string.IsNullOrWhiteSpace(database))
                throw new AdministrationException("Falta configurar MongoDb:Database en el servidor.");
            return provider.GetRequiredService<IMongoClient>().GetDatabase(database);
        });
        services.AddSingleton<ITezStore, MongoTezStore>();
        services.AddScoped<TezService>();
        services.AddSingleton<IRegionCorporalStore, MongoRegionCorporalStore>();
        services.AddScoped<RegionCorporalService>();
        return services;
    }
}
