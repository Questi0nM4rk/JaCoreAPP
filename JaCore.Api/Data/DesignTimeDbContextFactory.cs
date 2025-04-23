using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace JaCore.Api.Data // Ensure this namespace matches your ApplicationDbContext location
{
    /// <summary>
    /// Provides a way for Entity Framework Core tools (like migrations) to create an
    /// ApplicationDbContext instance at design time without needing to run the full application startup.
    /// It manually builds configuration to find the connection string.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Get environment variable to determine which appsettings file to use, default to Development
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // Build configuration manually, focusing only on what's needed for the DbContext
            IConfigurationRoot configuration = new ConfigurationBuilder()
                // Set base path to the JaCore.Api project directory where appsettings reside
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../JaCore.Api")) // Adjust if EF tools run from solution root
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true) // Load environment-specific settings
                .AddEnvironmentVariables() // Load environment variables (might override json)
                .AddUserSecrets<Program>(optional: true) // Load user secrets (important for sensitive data like connection strings or JWT secrets if used here)
                .Build();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException($"Could not find a connection string named 'DefaultConnection'. Ensure it's set in appsettings.json, appsettings.{environment}.json, user secrets, or environment variables.");
            }

            Console.WriteLine($"DesignTimeDbContextFactory: Using connection string: {connectionString.Substring(0, connectionString.IndexOf("Password=")) + "Password=********"}"); // Log connection string (hide password)

            builder.UseNpgsql(connectionString, options => options.EnableRetryOnFailure()); // Use PostgreSQL provider

            return new ApplicationDbContext(builder.Options);
        }
    }
}
