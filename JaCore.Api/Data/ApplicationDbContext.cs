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
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<DeviceOperation> DeviceOperations { get; set; }
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

        // --- Configure Device Module Entities ---

        // Device Entity Configuration
        builder.Entity<Device>(entity =>
        {
            entity.HasIndex(d => d.SerialNumber).IsUnique();
            entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
            entity.Property(d => d.SerialNumber).IsRequired().HasMaxLength(100);

            // Relationship: Device <-> Category (One-to-Many)
            entity.HasOne(d => d.Category)
                  .WithMany(c => c.Devices)
                  .HasForeignKey(d => d.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull); // Or Restrict, depending on requirements

            // Relationship: Device <-> Supplier (One-to-Many)
            entity.HasOne(d => d.Supplier)
                  .WithMany(s => s.Devices)
                  .HasForeignKey(d => d.SupplierId)
                  .OnDelete(DeleteBehavior.SetNull); // Or Restrict

            // Relationship: Device <-> DeviceCard (One-to-One)
            // Configured primarily from the DeviceCard side due to FK placement
            entity.HasOne(d => d.DeviceCard)
                  .WithOne(dc => dc.Device)
                  .HasForeignKey<DeviceCard>(dc => dc.DeviceId) // FK is in DeviceCard
                  .OnDelete(DeleteBehavior.Cascade); // If Device is deleted, delete its Card
        });

        // DeviceCard Entity Configuration
        builder.Entity<DeviceCard>(entity =>
        {
            entity.Property(dc => dc.Location).HasMaxLength(150);

            // One-to-One with Device is handled by the Device configuration above

            // Relationship: DeviceCard <-> Events (One-to-Many)
            entity.HasMany(dc => dc.Events)
                  .WithOne(e => e.DeviceCard)
                  .HasForeignKey(e => e.DeviceCardId)
                  .OnDelete(DeleteBehavior.Cascade); // If DeviceCard is deleted, delete its Events

            // Relationship: DeviceCard <-> DeviceOperations (One-to-Many)
            entity.HasMany(dc => dc.Operations)
                  .WithOne(op => op.DeviceCard)
                  .HasForeignKey(op => op.DeviceCardId)
                  .OnDelete(DeleteBehavior.Cascade); // If DeviceCard is deleted, delete its Operations
        });

        // Category Entity Configuration
        builder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Name).IsUnique();
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
        });

        // Supplier Entity Configuration
        builder.Entity<Supplier>(entity =>
        {
            entity.HasIndex(s => s.Name).IsUnique();
            entity.Property(s => s.Name).IsRequired().HasMaxLength(150);
        });

        // Service Entity Configuration
        builder.Entity<Service>(entity =>
        {
            entity.Property(svc => svc.Name).IsRequired().HasMaxLength(100);
            entity.Property(svc => svc.ProviderName).IsRequired().HasMaxLength(150);
        });

        // Event Entity Configuration
        builder.Entity<Event>(entity =>
        {
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Type).HasConversion<string>(); // Store Enum as string
        });

        // DeviceOperation Entity Configuration
        builder.Entity<DeviceOperation>(entity =>
        {
            entity.Property(op => op.OperationType).IsRequired().HasMaxLength(100);
            entity.Property(op => op.Status).IsRequired().HasMaxLength(50);
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
