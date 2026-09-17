using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NLDMAP.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Detectar si el sistema se está ejecutando local o en la nube (Producción)
        string databaseEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Desarrollo";
        string connectionString;

        if (databaseEnvironment == "Production")
        {
            // Entorno Nube: Se lee la URL segura que te dé el proveedor de base de datos
            connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
                               ?? throw new InvalidOperationException("La variable de entorno DATABASE_URL no está configurada.");
        }
        else
        {
            // Entorno Local: Se lee de tu archivo appsettings.json
            connectionString = configuration.GetConnectionString("LocalPostgresConnection") 
                               ?? throw new InvalidOperationException("La cadena de conexión local no fue encontrada.");
        }

        // 2. Registrar el Contexto de Base de Datos para Entity Framework Core
        // Nota: Cambia 'NldmapDbContext' por el nombre exacto del DbContext que use tu compañero si ya creó uno
        services.AddDbContext<DbContext>(options =>
            options.UseNpgsql(connectionString, sqlOptions =>
            {
                // Habilitar la traducción espacial nativa de PostGIS
                sqlOptions.UseNetTopologySuite();
                
                // Configurar reintentos automáticos por si la red de la nube llega a parpadear
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            }));

        return services;
    }
}