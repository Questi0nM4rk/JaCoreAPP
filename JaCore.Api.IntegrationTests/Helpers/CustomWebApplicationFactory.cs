using JaCore.Api.Data; // Your DbContext namespace
using JaCore.Api.IntegrationTests.Helpers; // Where DatabaseFixture lives
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost; // For ConfigureTestServices
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions; // For Remove / TryRemove
using Npgsql; // Npgsql EF Core provider namespace
using System.Data.Common; // For DbConnection
using Xunit; // Needed for IClassFixture

namespace JaCore.Api.IntegrationTests.Helpers;

// This factory is used by test classes to create a test server instance.
// It uses IClassFixture<DatabaseFixture> to get the shared database container.
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _dbFixture;

    // Constructor receives the DatabaseFixture instance managed by xUnit via the collection
    public CustomWebApplicationFactory(DatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
        Console.WriteLine("---> CustomWebApplicationFactory created, using shared DatabaseFixture.");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Console.WriteLine("---> Configuring WebHost for Testing...");
        // Use ConfigureTestServices for reliable service overrides in tests
        builder.ConfigureTestServices(services =>
        {
            Console.WriteLine("---> Configuring Test Services...");
            // Remove the original DbContext registration from Program.cs/Startup.cs
            var dbContextOptionsDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbContextOptionsDescriptor != null)
            {
                Console.WriteLine("---> Removing existing DbContextOptions<ApplicationDbContext> registration.");
                services.Remove(dbContextOptionsDescriptor);
            }

            // Also remove the DbContext registration itself if it exists
             var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ApplicationDbContext));
            if (dbContextDescriptor != null)
            {
                 Console.WriteLine("---> Removing existing ApplicationDbContext registration.");
                services.Remove(dbContextDescriptor);
            }

            // Remove any existing DbConnection registration that might conflict
            var dbConnectionDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection));
             if (dbConnectionDescriptor != null)
            {
                 Console.WriteLine("---> Removing existing DbConnection registration.");
                 services.Remove(dbConnectionDescriptor);
            }

            // Add ApplicationDbContext using the Testcontainer's dynamic connection string
            Console.WriteLine($"---> Adding ApplicationDbContext with Testcontainer Connection String: {_dbFixture.ConnectionString}");
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(_dbFixture.ConnectionString); // Use Npgsql and the fixture's connection string
                // options.EnableSensitiveDataLogging(); // Optional: Useful for debugging failed tests
            });
             Console.WriteLine("---> ApplicationDbContext configured for Testcontainer.");

            // Optional: Override other services for testing (e.g., mock external dependencies)
            // services.Replace(ServiceDescriptor.Scoped<IEmailService, MockEmailService>());
             Console.WriteLine("---> Test Services configuration complete.");
        });

        // Optionally set the environment for tests (useful if appsettings.Testing.json exists)
        builder.UseEnvironment("Testing");
        Console.WriteLine("---> WebHost configuration complete.");
    }

    // Helper to get a DbContext instance scoped to the test server, useful for setup/assertions
    // Be cautious using this directly in tests if the test modifies data, as it shares the
    // same transaction scope as the request unless handled carefully.
    public ApplicationDbContext GetScopedDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }
}
