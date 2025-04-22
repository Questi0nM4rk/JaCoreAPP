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

    // --- DTO Definitions ---
    public record RegisterUserDto(
        string Email,
        string FirstName,
        string LastName,
        string Password
    );

    public record LoginUserDto(
        string Email,
        string Password
    );

    // --- Common Helper Methods ---

    protected async Task<(AuthResponseDto? AuthResult, HttpResponseMessage Response)> RegisterUserAsync(
        string adminAccessToken,
        RegisterUserDto registerDto)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, RegisterUrl)
        {
            Content = JsonContent.Create(registerDto)
        };
        httpRequest.Headers.Authorization = new("Bearer", adminAccessToken);

        var response = await _client.SendAsync(httpRequest);
        AuthResponseDto? result = response.IsSuccessStatusCode 
            ? await response.Content.ReadFromJsonAsync<AuthResponseDto>()
            : null;

        Console.WriteLine($"Registration attempt for {registerDto.Email} - Status: {response.StatusCode}");
        return (result, response);
    }

    protected async Task<(AuthResponseDto? AuthResult, HttpResponseMessage Response)> LoginUserAsync(
        LoginUserDto loginDto)
    {
        var response = await _client.PostAsJsonAsync(LoginUrl, loginDto);
        AuthResponseDto? result = response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<AuthResponseDto>()
            : null;

        Console.WriteLine($"Login attempt for {loginDto.Email} - Status: {response.StatusCode}");
        return (result, response);
    }

    protected async Task<(AuthResponseDto? AuthResult, HttpResponseMessage Response)> RefreshTokenAsync(
        string refreshToken)
    {
        var response = await _client.PostAsJsonAsync(RefreshUrl, new { refreshToken });
        AuthResponseDto? result = response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<AuthResponseDto>()
            : null;

        Console.WriteLine($"Refresh token attempt - Status: {response.StatusCode}");
        return (result, response);
    }

    protected async Task<HttpResponseMessage> LogoutAsync(string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, LogoutUrl);
        request.Headers.Authorization = new("Bearer", accessToken);
        return await _client.SendAsync(request);
    }

    // --- Success-Oriented Helper Methods ---

    /// <summary>
    /// Registers a user (requires admin privileges) and asserts that the operation was successful.
    /// Throws an exception if registration fails or returns null data.
    /// </summary>
    /// <param name="adminAccessToken">The access token of an authenticated admin user.</param>
    /// <returns>The non-nullable AuthResponseDto on success.</returns>
    protected async Task<AuthResponseDto> RegisterUserSuccessfullyAsync(
        string adminAccessToken, // Added: Admin token is now required
        RegisterUserDto registerDto)
    {
        // Pass the admin token to the underlying helper
        var (result, response) = await RegisterUserAsync(adminAccessToken, registerDto);

        // Assert success within the helper
        response.EnsureSuccessStatusCode(); // Throws if not 2xx
        result.Should().NotBeNull("because successful registration must return auth data.");

        return result!;
    }

    /// <summary>
    /// Logs in a user and asserts that the operation was successful.
    /// Throws an exception if login fails or returns null data.
    /// </summary>
    /// <returns>The non-nullable AuthResponseDto on success.</returns>
    protected async Task<AuthResponseDto> LoginUserSuccessfullyAsync(LoginUserDto loginDto)
    {
        var (result, response) = await LoginUserAsync(loginDto);

        // Assert success within the helper
        response.EnsureSuccessStatusCode(); // Throws if not 2xx
        result.Should().NotBeNull("because successful login must return auth data.");
        // Null checks are already inside LoginUserAsync for success cases.

        return result!;
    }

    /// <summary>
    /// Logs in using the pre-seeded admin credentials and returns a valid access token.
    /// Assumes an admin user is seeded during test setup (e.g., in DatabaseFixture or TestDataSeeder).
    /// Throws exceptions if login fails.
    /// </summary>
    /// <returns>A valid access token for the pre-seeded admin user.</returns>
    protected async Task<string> GetAdminAccessTokenAsync()
    {
        // --- MODIFIED ---
        // Assume admin credentials are known and seeded
        // Replace with your actual seeded admin credentials
        var loginDto = new LoginUserDto(
            Email: "admin@jacore.app", // Replace with actual seeded admin email
            Password: "AdminPassword123!" // Replace with actual seeded admin password
        );

        Console.WriteLine($"---> Attempting to log in as seeded admin: {loginDto.Email}");

        // Login as the pre-seeded admin user
        var loginResult = await LoginUserSuccessfullyAsync(loginDto);

        loginResult.AccessToken.Should().NotBeNullOrWhiteSpace("because admin login should succeed and provide a token.");

        Console.WriteLine($"---> Successfully obtained access token for admin: {loginDto.Email}");
        return loginResult.AccessToken;
    }
}
