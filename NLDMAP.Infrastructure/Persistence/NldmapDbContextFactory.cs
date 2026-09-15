using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NLDMAP.Infrastructure.Persistence
{
//esta clase solo es detected y utilizada por el comando 'dotnet ef'
    public class NldmapDbContextFactory : IDesignTimeDbContextFactory<NldmapDbContext>
    {
        public NldmapDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NldmapDbContext>();

            var connectionString = "Host=localhost;Port=5433;Database=nldmap_db;Username=postgres;Password=admin1234";
            
            optionsBuilder.UseNpgsql(connectionString, x => x.UseNetTopologySuite());
            
            return new NldmapDbContext(optionsBuilder.Options);

        }
    }
}  