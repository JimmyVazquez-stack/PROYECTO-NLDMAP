using Microsoft.EntityFrameworkCore;
using NLDMAP.Domain.Entities;

namespace NLDMAP.Infrastructure.Persistence
{
    public class NldmapDbContext : DbContext
    {
        //Constructor para la inyeccion de dependencias
        public NldmapDbContext(DbContextOptions<NldmapDbContext> options) : base(options)   
        {
        }

        //Definicion de tablas en la BD
        public DbSet<User> Users { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<MissingPerson> MissingPersons{ get; set; }

        //Configuracion de las tablas - fluent api
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //configuracion en la tabla usuarios
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                
            });

            //Configuracion de tabla de reportes 
            modelBuilder.Entity<Report>(builder =>
            {
                builder.HasKey(r => r.Id);
                
                //Reporte asignado a una persona
                builder.HasOne<MissingPerson>()
                    .WithMany()
                    .HasForeignKey(r => r.MissingPersonId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                //reporte asignado al usuario creador
                builder.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                //estado del reporte enum
                builder.Property<ReportStatus>(r => r.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

                //configurar value object
                builder.OwnsOne(r => r.Events);

            });
        }
    }
}