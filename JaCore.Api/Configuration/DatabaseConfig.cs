using JaCore.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JaCore.Api.Configuration;

public static class DatabaseConfig
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // REMOVED: Check for existing DbContext provider
        // var serviceProvider = services.BuildServiceProvider();
        // var dbContextOptions = serviceProvider.GetService<DbContextOptions<ApplicationDbContext>>();
        // var existingProvider = dbContextOptions?.Extensions.OfType<CoreOptionsExtension>().FirstOrDefault()?.GetType();
        
        // if (existingProvider == null) // Always configure in Program.cs, let factory override
        // {
        //     // Only configure DB if no provider is already registered
        //     if (environment.IsDevelopment() || environment.IsEnvironment("Test"))
        //     {
        //         // Use in-memory database for development/test
        //         services.AddDbContext<ApplicationDbContext>(options =>
        //             options.UseInMemoryDatabase("JaCoreDb")); // Use a consistent name if factory isn't overriding
        //     }
        //     else
        //     {
        //         // Use SQL Server for production
        //         services.AddDbContext<ApplicationDbContext>(options =>
        //             options.UseSqlServer(configuration.GetConnectionString("DefaultConnection") ?? 
        //                                  throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));
        //     }
        // }

        // Simplified: Always register the default DB provider based on environment.
        // The WebApplicationFactory will remove/replace this if it configures its own.
        if (environment.IsDevelopment() || environment.IsEnvironment("Test"))
        {
            // Use in-memory database for development/test by default
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("JaCoreDb_Default")); // Use a distinct default name
        }
        else
        {
            // Use SQL Server for production
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection") ?? 
                                     throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));
        }

        return services;
    }

    public static void SeedDatabase(IServiceProvider serviceProvider, IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment()) return;
        
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
        // Add sample categories if none exist
        if (!dbContext.Categories.Any())
        {
            dbContext.Categories.AddRange(
                new Models.Device.Category { Name = "Laboratory Equipment" },
                new Models.Device.Category { Name = "Medical Devices" },
                new Models.Device.Category { Name = "Industrial Tools" },
                new Models.Device.Category { Name = "Office Equipment" }
            );
            dbContext.SaveChanges();
        }

        // Add sample services if none exist
        if (!dbContext.Services.Any())
        {
            dbContext.Services.AddRange(
                new Models.Device.Service { Name = "Technical Support", Contact = "support@example.com" },
                new Models.Device.Service { Name = "Maintenance", Contact = "maintenance@example.com" },
                new Models.Device.Service { Name = "Calibration", Contact = "calibration@example.com" }
            );
            dbContext.SaveChanges();
        }

        // Add sample suppliers if none exist
        if (!dbContext.Suppliers.Any())
        {
            dbContext.Suppliers.AddRange(
                new Models.Device.Supplier { Name = "Lab Supplies Inc.", Contact = "sales@labsupplies.com" },
                new Models.Device.Supplier { Name = "Medical Equipment Corp.", Contact = "info@medequip.com" },
                new Models.Device.Supplier { Name = "Office Solutions", Contact = "contact@officesolutions.com" }
            );
            dbContext.SaveChanges();
        }
    }
} 