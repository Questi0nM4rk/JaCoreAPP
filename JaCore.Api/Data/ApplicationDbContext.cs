using JaCore.Api.Models.Device;
using JaCore.Api.Models.User;
// using JaCore.Api.Models.Location; // Commented out temporarily
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace JaCore.Api.Data;

/// <summary>
/// Application database context
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    /// <summary>
    /// Initializes a new instance of the ApplicationDbContext
    /// </summary>
    /// <param name="options">The options for this context</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    /// <summary>
    /// Device entities
    /// </summary>
    public DbSet<Device> Devices { get; set; }
    
    /// <summary>
    /// Device card entities
    /// </summary>
    public DbSet<DeviceCard> DeviceCards { get; set; }
    
    /// <summary>
    /// Device operation entities
    /// </summary>
    public DbSet<DeviceOperation> DeviceOperations { get; set; }
    
    /// <summary>
    /// Device services
    /// </summary>
    public DbSet<Service> Services { get; set; }
    
    /// <summary>
    /// Device suppliers
    /// </summary>
    public DbSet<Supplier> Suppliers { get; set; }
    
    /// <summary>
    /// Device categories
    /// </summary>
    public DbSet<Category> Categories { get; set; }
    
    /// <summary>
    /// Metrological conformations
    /// </summary>
    public DbSet<MetrologicalConformation> MetrologicalConformations { get; set; }
    
    /// <summary>
    /// Device events
    /// </summary>
    public DbSet<Event> Events { get; set; }
    
    /// <summary>
    /// Configure the model
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure devices
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasOne<Category>()
                .WithMany()
                .HasForeignKey(d => d.CategoryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(d => d.DeviceCard)
                  .WithOne(dc => dc.Device)
                  .HasForeignKey<Device>(d => d.DeviceCardId)
                  .IsRequired(false);
        });
        
        // Configure device cards
        modelBuilder.Entity<DeviceCard>(entity =>
        {
            entity.HasOne(dc => dc.Service)
                  .WithMany()
                  .HasForeignKey(dc => dc.ServiceId)
                  .IsRequired(false);
                  
            entity.HasOne(dc => dc.Supplier)
                  .WithMany()
                  .HasForeignKey(dc => dc.SupplierId)
                  .IsRequired(false);
                  
            entity.HasOne(dc => dc.MetrologicalConformation)
                  .WithMany()
                  .HasForeignKey(dc => dc.MetrologicalConformationId)
                  .IsRequired(false);
        });
        
        // Configure device operations
        modelBuilder.Entity<DeviceOperation>(entity =>
        {
            entity.HasOne<Device>()
                .WithMany()
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
} 