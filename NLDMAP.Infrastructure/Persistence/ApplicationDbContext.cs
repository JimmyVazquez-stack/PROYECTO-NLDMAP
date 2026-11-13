using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore; 
using NLDMAP.Infrastructure.Identity;
using NLDMAP.Domain.Accounts;
using NLDMAP.Domain.Auditing;

namespace NLDMAP.Infrastructure.Persistence
{
    public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
        public DbSet<AccountInvitation> AccountInvitations => Set<AccountInvitation>();
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>().Property(u => u.DisplayName).HasMaxLength(120);
            builder.Entity<ApplicationUser>().Property(u => u.IsActive).HasDefaultValue(true);
            builder.Entity<AuditEntry>(e =>
            {
                e.HasIndex(x => x.OccurredAtUtc);
                e.HasIndex(x => x.TargetId);
                e.Property(x => x.ActorId).HasMaxLength(450);
                e.Property(x => x.ActorName).HasMaxLength(256);
                e.Property(x => x.Operation).HasMaxLength(80);
                e.Property(x => x.TargetType).HasMaxLength(40);
                e.Property(x => x.TargetId).HasMaxLength(450);
                e.Property(x => x.TargetName).HasMaxLength(256);
            });
            builder.Entity<AccountInvitation>(e =>
            {
                e.HasIndex(x => x.TokenHash).IsUnique();
                e.HasIndex(x => x.NormalizedEmail);
                e.Property(x => x.Email).HasMaxLength(256);
                e.Property(x => x.NormalizedEmail).HasMaxLength(256);
                e.Property(x => x.DisplayName).HasMaxLength(120);
                e.Property(x => x.TokenHash).HasMaxLength(64);
                e.Property(x => x.CreatedById).HasMaxLength(450);
                e.Property(x => x.DeliveryStatus).HasMaxLength(30);
            });
        }
    }
}
