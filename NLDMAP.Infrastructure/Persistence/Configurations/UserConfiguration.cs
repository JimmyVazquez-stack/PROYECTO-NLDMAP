using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NLDMAP.Domain.Entities;


namespace NLDMAP.Infrastructure.Persistence.Configurations
{

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            //email unico
            builder.HasIndex(e => e.Email).IsUnique();
            
            //optimizacion espacio en postgresql
            builder.Property(u => u.Email)
                .HasMaxLength(150)
                .IsRequired();
        }
    }
}