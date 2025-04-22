using FluentAssertions; // Added for Should().NotBeNull()
using JaCore.Api.DTOs.Auth;
using JaCore.Api.Helpers; // For ApiConstants
using JaCore.Api.IntegrationTests.Helpers; // For Factory, Collection
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;
using JaCore.Api.Data; // For ApplicationDbContext
using JaCore.Api.Entities.Identity; // For ApplicationUser
using JaCore.Common; // For RoleConstants
using Microsoft.AspNetCore.Identity; // For UserManager
using Microsoft.Extensions.DependencyInjection; // For CreateScope

namespace JaCore.Api.IntegrationTests.Controllers.Auth;

// Base class for Auth controller tests, sharing the database fixture via collection
[Collection("Database Collection")]
public abstract class AuthTestsBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient _client;
    protected readonly CustomWebApplicationFactory _factory;

    // Base URL for Auth controller
    protected const string AuthBaseUrl = $"{ApiConstants.ApiRoutePrefix}/auth"; // e.g., "/api/v1/auth"

    // Specific endpoint routes
    protected const string RegisterUrl = $"{AuthBaseUrl}/{ApiConstants.Routes.Register}";
    protected const string LoginUrl = $"{AuthBaseUrl}/{ApiConstants.Routes.Login}";
    protected const string RefreshUrl = $"{AuthBaseUrl}/{ApiConstants.Routes.Refresh}";
    protected const string LogoutUrl = $"{AuthBaseUrl}/{ApiConstants.Routes.Logout}";
    protected const string MeUrl = $"{AuthBaseUrl}/{ApiConstants.Routes.GetCurrentUser}";
    protected const string AdminOnlyUrl = $"{AuthBaseUrl}/{ApiConstants.Routes.AdminOnlyData}";

    protected AuthTestsBase(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        Console.WriteLine($"---> AuthTestsBase initialized for {GetType().Name}.");
    }

    // --- Common Helper Methods ---

    protected async Task<(AuthResponseDto? AuthResult, HttpResponseMessage Response)> RegisterUserAsync(
        string? email = null,
        string? password = null,
        string? firstName = "Test",
        string? lastName = "User")
    {
        var request = new RegisterDto(
            Email: email ?? $"test-{Guid.NewGuid()}@example.com",
            FirstName: firstName ?? "Test",
            LastName: lastName ?? "User",
            Password: password ?? "Password123!" // Ensure this meets policy
        );
        Console.WriteLine($"---> Helper: Attempting registration for: {request.Email} at {RegisterUrl}");
        var response = await _client.PostAsJsonAsync(RegisterUrl, request);
        AuthResponseDto? result = null;
        if (response.IsSuccessStatusCode)
        {
            result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            // Add assertion here for successful cases
            result.Should().NotBeNull("because registration is expected to succeed and return data.");
        }
        else
        {
             Console.WriteLine($"---> Helper: Registration failed: {response.StatusCode}");
             // Optionally log error content here if needed for debugging helpers
        }
        return (result, response);
    }

     protected async Task<(AuthResponseDto? AuthResult, HttpResponseMessage Response)> LoginUserAsync(string email, string password)
    {
        var request = new LoginDto(email, password);
        Console.WriteLine($"---> Helper: Attempting login for: {email} at {LoginUrl}");
        var response = await _client.PostAsJsonAsync(LoginUrl, request);
        AuthResponseDto? result = null;
        if (response.IsSuccessStatusCode)
        {
            result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            // Add assertion here for successful cases
            result.Should().NotBeNull("because login is expected to succeed and return data.");
            result!.RefreshToken.Should().NotBeNullOrWhiteSpace("because a successful login should provide a refresh token.");
        }
         else
        {
             Console.WriteLine($"---> Helper: Login failed: {response.StatusCode}");
        }
        return (result, response);
    }

    // --- Success-Oriented Helper Methods ---

    /// <summary>
    /// Registers a user and asserts that the operation was successful.
    /// Throws an exception if registration fails or returns null data.
    /// </summary>
    /// <returns>The non-nullable AuthResponseDto on success.</returns>
    protected async Task<AuthResponseDto> RegisterUserSuccessfullyAsync(
        string? email = null,
        string? password = null,
        string? firstName = "Test",
        string? lastName = "User")
    {
        var (result, response) = await RegisterUserAsync(email, password, firstName, lastName);

        // Assert success within the helper
        response.EnsureSuccessStatusCode(); // Throws if not 2xx
        result.Should().NotBeNull("because successful registration must return auth data.");
        // The null check is already inside RegisterUserAsync for success cases, but double-checking doesn't hurt
        // and makes the contract of this method clearer.

        return result!;
    }

    /// <summary>
    /// Logs in a user and asserts that the operation was successful.
    /// Throws an exception if login fails or returns null data.
    /// </summary>
    /// <returns>The non-nullable AuthResponseDto on success.</returns>
    protected async Task<AuthResponseDto> LoginUserSuccessfullyAsync(string email, string password)
    {
        var (result, response) = await LoginUserAsync(email, password);

        // Assert success within the helper
        response.EnsureSuccessStatusCode(); // Throws if not 2xx
        result.Should().NotBeNull("because successful login must return auth data.");
        // Null checks are already inside LoginUserAsync for success cases.

        return result!;
    }

    /// <summary>
    /// Creates a new user, promotes them to the Admin role directly via UserManager,
    /// logs them in, and returns a valid access token for that admin user.
    /// Throws exceptions if any step fails.
    /// </summary>
    /// <returns>A valid access token for an admin user.</returns>
    protected async Task<string> GetAdminAccessTokenAsync()
    {
        // Strategy: Create a user and promote them to Admin directly via DbContext/UserManager
        var adminEmail = $"admin-{Guid.NewGuid()}@example.com";
        var adminPassword = "Password123!";

        // 1. Register the user normally using the success helper
        var registerResult = await RegisterUserSuccessfullyAsync(adminEmail, adminPassword, "Admin", "User");
        var userId = registerResult.UserId;

        // 2. Use the factory's service provider to get UserManager
        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            userId.Should().NotBeNullOrWhiteSpace();
            // Find the user
            var user = await userManager.FindByIdAsync(userId.ToString()) ?? throw new InvalidOperationException($"Failed to find newly registered user with ID {userId} to promote to admin.");

            // 3. Add the user to the Admin role
            var addToRoleResult = await userManager.AddToRoleAsync(user, RoleConstants.Roles.Admin);
            if (!addToRoleResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to add user {userId} to Admin role: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
            }
            Console.WriteLine($"---> Promoted user {adminEmail} to Admin role.");
        }

        // 4. Login as the now-admin user using the success helper
        var loginResult = await LoginUserSuccessfullyAsync(adminEmail, adminPassword);

        loginResult.AccessToken.Should().NotBeNullOrWhiteSpace();

        return loginResult.AccessToken;
    }
}
