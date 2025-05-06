using JaCore.Api.Entities.Auth;
using JaCore.Api.Entities.Device;
using JaCore.Api.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JaCore.Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    // --- Device Module DbSets ---
    public DbSet<Device> Devices { get; set; }
    public DbSet<DeviceCard> DeviceCards { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Supplier> Suppliers { get; set; } = null!;
    public DbSet<Service> Services { get; set; } = null!;
    public DbSet<Event> Events { get; set; }
    public DbSet<DeviceOperation> DeviceOperations { get; set; }
    public DbSet<DeviceEvent> DeviceEvents { get; set; } = null!;
    // --- End Device Module DbSets ---

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Important for Identity schema

        // Configure RefreshToken entity
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.TokenHash).IsRequired();
            entity.Property(rt => rt.ExpiryDate).IsRequired();

            // Relationship: One User can have potentially many RefreshTokens over time
            entity.HasOne(rt => rt.User)
                  .WithMany() // No navigation property on User needed for this side
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade); // If user is deleted, cascade delete their refresh tokens

            // Optional index for performance
            entity.HasIndex(rt => rt.UserId);
        });
    }

    // Optional: Override SaveChangesAsync to automatically update timestamps
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    private void SetTimestamps()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && (
                    e.State == EntityState.Added
                    || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var now = DateTimeOffset.UtcNow;
            ((BaseEntity)entityEntry.Entity).ModifiedAt = now;

            if (entityEntry.State == EntityState.Added)
            {
                ((BaseEntity)entityEntry.Entity).CreatedAt = now;
            }
        }
    }
}
