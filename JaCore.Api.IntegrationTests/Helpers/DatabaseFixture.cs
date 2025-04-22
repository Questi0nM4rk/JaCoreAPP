using JaCore.Api.Data; // Ensure this namespace matches your DbContext location
using Microsoft.EntityFrameworkCore;
using Npgsql; // Npgsql EF Core provider namespace
using Testcontainers.PostgreSql; // Testcontainers Postgres module
using Xunit;

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

        // Apply EF Core migrations to the newly started container database
        try
        {
            Console.WriteLine("---> Applying Database Migrations...");
            await using var dbContext = CreateDbContext();
            await dbContext.Database.MigrateAsync();
            Console.WriteLine("---> Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"---> ERROR applying migrations: {ex.GetType().Name} - {ex.Message}");
            Console.WriteLine($"---> StackTrace: {ex.StackTrace}");
            throw; // Fail fast if migrations are essential
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
}
