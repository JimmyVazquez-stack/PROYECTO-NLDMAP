using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NLDMAP.Infrastructure.Identity;
using NLDMAP.Infrastructure.Persistence.Auditing;

namespace NLDMAP.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserProfileAuditEntry> UserProfileAuditEntries =>
        Set<UserProfileAuditEntry>();

    public DbSet<UserAccountStatusAuditEntry> UserAccountStatusAuditEntries =>
        Set<UserAccountStatusAuditEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserAccountStatusAuditEntry>(entry =>
        {
            entry.ToTable("UserAccountStatusAuditEntries");
            entry.HasKey(item => item.Id);
            entry.Property(item => item.Id).ValueGeneratedOnAdd();
            entry.Property(item => item.UserId).HasMaxLength(450).IsRequired();
            entry.Property(item => item.ActorUserId).HasMaxLength(450).IsRequired();
            entry.Property(item => item.ActorDisplayName).IsRequired();
            entry.Property(item => item.OccurredAtUtc).IsRequired();
            entry.Property(item => item.Reason).HasMaxLength(1000).IsRequired();
            entry.HasIndex(item => new { item.UserId, item.Id });
        });

        builder.Entity<UserProfileAuditEntry>(entry =>
        {
            entry.ToTable("UserProfileAuditEntries");
            entry.HasKey(item => item.Id);
            entry.Property(item => item.Id).ValueGeneratedOnAdd();
            entry.Property(item => item.UserId).HasMaxLength(450).IsRequired();
            entry.Property(item => item.ActorUserId).HasMaxLength(450).IsRequired();
            entry.Property(item => item.ActorDisplayName).IsRequired();
            entry.Property(item => item.OccurredAtUtc).IsRequired();
            entry.Property(item => item.PreviousFirstName).IsRequired();
            entry.Property(item => item.PreviousLastName).IsRequired();
            entry.Property(item => item.NewFirstName).IsRequired();
            entry.Property(item => item.NewLastName).IsRequired();
            entry.HasIndex(item => new { item.UserId, item.Id });

            // Keep audit IDs independent from account deletion and avoid cascading removal.
        });

        builder.Entity<ApplicationUser>(user =>
        {
            user.Property(account => account.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Existing accounts can receive these columns without losing their data.
            user.Property(account => account.FirstName)
                .IsRequired()
                .HasDefaultValue(string.Empty);

            user.Property(account => account.LastName)
                .IsRequired()
                .HasDefaultValue(string.Empty);

            // Enforce email uniqueness even when registration requests overlap.
            user.HasIndex(account => account.NormalizedEmail)
                .HasDatabaseName("EmailIndex")
                .IsUnique();
        });
    }
}
