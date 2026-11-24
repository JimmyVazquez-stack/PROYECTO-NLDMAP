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
        services.AddSingleton<ISepomexStore, MongoSepomexStore>();
        services.AddScoped<SepomexService>();
        services.AddScoped<CatalogAccess>();
        services.AddSingleton<IGeneroStore, MongoGeneroStore>();
        services.AddScoped<GeneroService>();
        services.AddSingleton<IOrientacionSexualStore, MongoOrientacionSexualStore>();
        services.AddScoped<OrientacionSexualService>();
        services.AddSingleton<ICircunstanciaStore, MongoCircunstanciaStore>();
        services.AddScoped<CircunstanciaService>();
        services.AddSingleton<INacionalidadStore, MongoNacionalidadStore>();
        services.AddScoped<NacionalidadService>();
        services.AddSingleton<IParentescoStore, MongoParentescoStore>();
        services.AddScoped<ParentescoService>();
        services.AddSingleton<IDiscapacidadStore, MongoDiscapacidadStore>();
        services.AddScoped<DiscapacidadService>();
        services.AddSingleton<IComplexionStore, MongoComplexionStore>();
        services.AddScoped<ComplexionService>();
        services.AddScoped<MongoCatalogSeeder>();
        services.AddSingleton<IOcupacionStore, MongoOcupacionStore>();
        services.AddScoped<OcupacionService>();
        services.AddSingleton<IVehiculoStore, MongoVehiculoStore>();
        services.AddScoped<VehiculoService>();
        services.AddSingleton<ITipoCabelloStore, MongoTipoCabelloStore>();
        services.AddScoped<TipoCabelloService>();
        services.AddSingleton<IColoresCatalogoStore, MongoColoresCatalogoStore>();
        services.AddScoped<ColoresCatalogoService>();
        services.AddSingleton<ITipoCalzadoStore, MongoTipoCalzadoStore>();
        services.AddScoped<TipoCalzadoService>();
        services.AddSingleton<ITipoPrendaStore, MongoTipoPrendaStore>();
        services.AddScoped<TipoPrendaService>();
        services.AddSingleton<ITezStore, MongoTezStore>();
        services.AddScoped<TezService>();
        services.AddSingleton<IRegionCorporalStore, MongoRegionCorporalStore>();
        services.AddScoped<RegionCorporalService>();
        services.AddSingleton<ITipoSenaStore, MongoTipoSenaStore>();
        services.AddScoped<TipoSenaService>();
        return services;
    }
}
