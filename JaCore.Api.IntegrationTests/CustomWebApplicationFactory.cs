using JaCore.Api.Data;
using JaCore.Api.Models.User;
using JaCore.Api.Dtos.User;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace JaCore.Api.IntegrationTests;

public class CustomWebApplicationFactory<TProgram> 
    : WebApplicationFactory<TProgram> where TProgram : class
{
    // Store constants for test user credentials
    public const string DefaultPassword = "Password123!";
    public const string AdminEmail = "admin@test.com";
    public const string ManagementEmail = "manager@test.com";
    public const string UserEmail = "user@test.com";
    public const string DebugEmail = "debug@test.com";

    public IConfiguration Configuration { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use a test environment name
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((context, conf) =>
        {
            // Add appsettings.json and appsettings.Development.json
            conf.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            conf.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true,
                reloadOnChange: true);
            Configuration = conf.Build(); // Build the configuration here
        });

        builder.ConfigureServices(services =>
        {
            // Remove the app's ApplicationDbContext registration.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add ApplicationDbContext using an in-memory database for testing.
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });
        });
    }

    // Method to get an authenticated client
    public async Task<HttpClient> GetAuthenticatedClientAsync(string role = "User")
    {
        var client = CreateClient();
        var user = TestUsers.GetTestUser(role); // Ensure you have a TestUsers class or similar

        var loginDto = new UserLoginDto { Email = user.Email!, Password = ApiWebApplicationFactory.DefaultPassword };

        // Post the login details
        var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Check for successful login, handle potential errors more gracefully
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Login failed: {response.StatusCode}, Content: {errorContent}"); // Log error
            response.EnsureSuccessStatusCode(); // This will throw if login failed
        }

        // Read the token from the response
        var responseContent = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            throw new InvalidOperationException("Login response content was empty.");
        }

        AuthResponseDto? authResponse;
        try
        {
            authResponse = JsonSerializer.Deserialize<AuthResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize login response: {responseContent}", ex);
        }


        if (authResponse == null || string.IsNullOrWhiteSpace(authResponse.Token))
        {
            throw new InvalidOperationException("Token was null or empty in the login response.");
        }

        // Set the authorization header for subsequent requests
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);

        return client;
    }

    private static async Task InitializeDbForTests(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        // Seed roles
        var roles = new[] { "Admin", "User", "Management", "Debug" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }

        // Seed users
        foreach (var roleName in roles)
        {
            var user = TestUsers.GetTestUser(roleName);
            var existingUser = await userManager.FindByEmailAsync(user.Email!);
            if (existingUser == null)
            {
                var result = await userManager.CreateAsync(user, ApiWebApplicationFactory.DefaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, roleName);
                }
                else
                {
                    // Log or handle user creation failure
                    Console.WriteLine(
                        $"Failed to create user {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else if (!await userManager.IsInRoleAsync(existingUser, roleName))
            {
                // Ensure existing user is in the correct role for the test context
                await userManager.AddToRoleAsync(existingUser, roleName);
            }
        }
    }
}

public class ApiWebApplicationFactory : CustomWebApplicationFactory<Program> { } 