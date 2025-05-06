using JaCore.Api.Data; // Ensure this namespace matches your DbContext location
using Microsoft.EntityFrameworkCore;
using Npgsql; // Npgsql EF Core provider namespace
using Testcontainers.PostgreSql; // Testcontainers Postgres module
using Xunit;
using Microsoft.Extensions.DependencyInjection; // Ensure this is present
using Microsoft.AspNetCore.Identity; // Ensure this is present
using JaCore.Api.Entities.Identity; // Ensure this is present
using JaCore.Api.Helpers; // Keep for TestDataSeeder
using Microsoft.Extensions.Hosting; // Ensure this is present
using Microsoft.Extensions.Logging; // Ensure this is present
using Microsoft.Extensions.FileProviders; // Ensure this is present

namespace JaCore.Api.IntegrationTests.Helpers;

// Manages a PostgreSQL container shared by tests within a single test class collection.
// Using CollectionDefinition allows sharing the fixture across multiple test classes.
[CollectionDefinition("Database Collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

public class DatabaseFixture : IAsyncLifetime
{
    // Configure the container
    public PostgreSqlContainer PostgresContainer { get; } = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine") // Use a specific, stable version
        .WithDatabase("jacore_test_db")  // Distinct name for the test DB
        .WithUsername("testuser")
        .WithPassword("testpassword")
        .WithCleanUp(true) // Ensures container is removed after tests
        .Build();

    // Publicly exposes the dynamic connection string
    public string ConnectionString => PostgresContainer.GetConnectionString();

    // Called once before any tests in the collection run
    public async Task InitializeAsync()
    {
        Console.WriteLine("---> Starting PostgreSQL Testcontainer for collection...");
        await PostgresContainer.StartAsync();
        Console.WriteLine($"---> PostgreSQL Testcontainer started on: {PostgresContainer.Hostname}:{PostgresContainer.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort)}");
        Console.WriteLine($"---> Connection String: {ConnectionString}");

        // Apply EF Core migrations and Seed Data
        try
        {
            Console.WriteLine("---> Applying Database Migrations and Seeding Data...");

            // Create DbContext directly for migrations
            await using var migrateDbContext = CreateDbContext();
            await migrateDbContext.Database.MigrateAsync();
            Console.WriteLine("---> Database migrations applied successfully.");

            // Seed Data - Requires resolving UserManager/RoleManager
            var services = new ServiceCollection();
            services.AddLogging(logging => logging.AddConsole()); // Enable console logging (more verbose)
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(ConnectionString));
            
            // Add Identity with debug options
            services.AddIdentityCore<ApplicationUser>(options => {
                // Debug: Disable password complexity for tests to ensure it's not the password validation failing
                options.Password.RequiredLength = 6; // Lower than production
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 0;
                
                // User settings
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false; // Ensure NOT requiring confirmation
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
            
            // Use string "Test" for environment name
            services.AddSingleton<IHostEnvironment>(new MockHostEnvironment { EnvironmentName = "Test" });

            await using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var env = scopedServices.GetRequiredService<IHostEnvironment>();

            // Before seeding, check if admin exists already
            var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
            var adminUser = await userManager.FindByEmailAsync("admin@jacore.app");
            if (adminUser != null) {
                Console.WriteLine("---> Admin user already exists, checking settings...");
                Console.WriteLine($"---> Admin IsActive: {adminUser.IsActive}");
                Console.WriteLine($"---> Admin EmailConfirmed: {adminUser.EmailConfirmed}");
                
                // Check password validation directly - useful for debug
                var isPasswordValid = await userManager.CheckPasswordAsync(adminUser, "AdminPassword123!");
                Console.WriteLine($"---> Admin password validation result: {isPasswordValid}");
                
                // Check roles
                var roles = await userManager.GetRolesAsync(adminUser);
                Console.WriteLine($"---> Admin roles: {string.Join(", ", roles)}");
            } else {
                Console.WriteLine("---> Admin user doesn't exist yet, will create during seeding");
            }

            // Do the seeding
            await TestDataSeeder.SeedEssentialUsersAsync(scopedServices, env);
            
            // Verify after seeding
            adminUser = await userManager.FindByEmailAsync("admin@jacore.app");
            if (adminUser != null) {
                Console.WriteLine("---> Admin user exists after seeding!");
                Console.WriteLine($"---> Admin IsActive: {adminUser.IsActive}");
                Console.WriteLine($"---> Admin EmailConfirmed: {adminUser.EmailConfirmed}");
                
                // Validate password again
                var isPasswordValid = await userManager.CheckPasswordAsync(adminUser, "AdminPassword123!");
                Console.WriteLine($"---> Admin password validation result after seeding: {isPasswordValid}");
                
                // Check roles after seeding
                var roles = await userManager.GetRolesAsync(adminUser);
                Console.WriteLine($"---> Admin roles after seeding: {string.Join(", ", roles)}");
            } else {
                Console.WriteLine("---> CRITICAL: Admin user still missing after seeding!");
            }
            
            Console.WriteLine("---> Test data seeding completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"---> ERROR applying migrations or seeding data: {ex.GetType().Name} - {ex.Message}");
            Console.WriteLine($"---> StackTrace: {ex.StackTrace}");
            // Log inner exceptions if present
            var innerEx = ex.InnerException;
            while (innerEx != null)
            {
                 Console.WriteLine($"---> Inner Exception: {innerEx.GetType().Name} - {innerEx.Message}");
                 innerEx = innerEx.InnerException;
            }
            throw; // Fail fast
        }
    }

    // Called once after all tests in the collection have run
    public async Task DisposeAsync()
    {
        Console.WriteLine("---> Disposing PostgreSQL Testcontainer for collection...");
        await PostgresContainer.DisposeAsync(); // Stops and removes the container
        Console.WriteLine("---> PostgreSQL Testcontainer disposed.");
    }

    // Helper method to create a DbContext instance connected to the test container
    // This can be used directly in tests for setup/assertion if needed, but the
    // factory handles the main wiring for the application itself.
    public ApplicationDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(ConnectionString, options => options.EnableRetryOnFailure());
        // optionsBuilder.EnableSensitiveDataLogging(); // Uncomment for debugging
        return new ApplicationDbContext(optionsBuilder.Options);
    }

    // Simple mock IHostEnvironment for testing purposes
    private class MockHostEnvironment : IHostEnvironment
    {
        // Use string "Test"
        public string EnvironmentName { get; set; } = "Test";
        public string ApplicationName { get; set; } = string.Empty;
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
