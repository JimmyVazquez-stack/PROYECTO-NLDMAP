using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NLDMAP.Domain.Entities;

namespace NLDMAP.Infrastructure.Persistence.Configurations
{

    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            //llave primaria
            builder.HasKey(r => r.Id);

            //relacion 1 a N, reporte pertenece a una persona desaparecida
            builder.HasOne<MissingPerson>()
                .WithMany(p => p.ReportHistory)
                .HasForeignKey(r => r.MissingPersonId)
                .OnDelete(DeleteBehavior.Restrict);
            
            //relacion 1 a N, el reporte es creado por un ciudadano (usuario)
            builder.HasOne<User>()
                .WithMany() // vacio por si usuario no necesita cargar todo su historial
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            //Mapeo enum a string para legibilidad
            builder.Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            
            //Configuracion limites strings para tabla principal
            builder.Property(r => r.MissingPersonRelation)
                .HasMaxLength(100)
                .IsRequired();
            
            //Mapeo del objeto de valor
            builder.OwnsOne(r => r.Events, eventsBuilder =>
            {
                //fechas y tiempos
                eventsBuilder.Property(e => e.EventsDate).HasColumnName("Event_Date");
                eventsBuilder.Property(e => e.EventsHour).HasColumnName("Event_Hour");
                
                //circunstancias
                eventsBuilder.Property(e => e.Circumstance)
                    .HasColumnName("Event_Circumstance")
                    .HasColumnType("text"); //permitir textos largos

                eventsBuilder.Property(e => e.ReporterPresent)
                    .HasColumnName("ReporterPresent");
                
                //georreferenciacion
                eventsBuilder.Property(e => e.Street)
                    .HasColumnName("Address_Street")
                    .HasMaxLength(150);

                eventsBuilder.Property(e => e.City)
                    .HasColumnName("City")
                    .HasMaxLength(150);

                eventsBuilder.Property(e => e.State)
                    .HasColumnName("Address_State")
                    .HasMaxLength(100);
                
                eventsBuilder.Property(e => e.Municipality)
                    .HasColumnName("Address_Municipality")
                    .HasMaxLength(150);
               
                //georreferenciacion espacial postgis/neo4j
                eventsBuilder.Property(e => e.Latitude)
                    .HasColumnName("Location_Latitude");
                eventsBuilder.Property(e => e.Longitude)
                    .HasColumnName("Location_Longitude");
            });
        }
    }
}