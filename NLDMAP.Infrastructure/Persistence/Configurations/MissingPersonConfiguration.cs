using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NLDMAP.Domain.Entities;

namespace NLDMAP.Infrastructure.Persistence.Configurations;

public class MissingPersonConfiguration : IEntityTypeConfiguration<MissingPerson>
{
    public void Configure(EntityTypeBuilder<MissingPerson> builder)
    {
        builder.HasKey(m => m.Id);
        
        //restricciones de longitud
        builder.Property(m => m.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.Curp)
            .HasMaxLength(18);

        builder.Property(m => m.Nationality)
            .HasMaxLength(50);

    }
}