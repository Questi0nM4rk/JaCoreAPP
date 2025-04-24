using FluentAssertions; // Added for Should().NotBeNull()
using JaCore.Api.Helpers; // For ApiConstants
using JaCore.Api.IntegrationTests.Helpers; // For Factory, Collection
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;
using JaCore.Api.IntegrationTests.DTOs.Auth; // Added new using for Integration Test DTOs

namespace JaCore.Api.IntegrationTests.Controllers.Base;

// Base class for Auth controller tests, sharing the database fixture via collection
[Collection("Database Collection")]
public abstract class AuthTestsBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient _client;
    protected readonly CustomWebApplicationFactory _factory;

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
        string adminAccessToken,
        RegisterDto registerDto) // Updated to use new RegisterDto
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiConstants.AuthRoutes.Register)
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
        LoginDto loginDto) // Updated to use new LoginDto
    {
        Console.WriteLine($"---> Attempting send login request to endpoint: {ApiConstants.AuthRoutes.Login}");
        var response = await _client.PostAsJsonAsync(ApiConstants.AuthRoutes.Login, loginDto);
        AuthResponseDto? result = response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<AuthResponseDto>()
            : null;

        Console.WriteLine($"Login attempt for {loginDto.Email} - Status: {response.StatusCode}");
        return (result, response);
    }

    protected async Task<(AuthResponseDto? AuthResult, HttpResponseMessage Response)> RefreshTokenAsync(
        string refreshToken)
    {
        var refreshDto = new TokenRefreshRequestDto(refreshToken); // Use new TokenRefreshRequestDto implicitly or explicitly
        var response = await _client.PostAsJsonAsync(ApiConstants.AuthRoutes.Refresh, refreshDto);
        AuthResponseDto? result = response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<AuthResponseDto>()
            : null;

        Console.WriteLine($"Refresh token attempt - Status: {response.StatusCode}");
        return (result, response);
    }

    protected async Task<HttpResponseMessage> LogoutAsync(string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, ApiConstants.AuthRoutes.Logout);
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
        RegisterDto registerDto) // Updated to use new RegisterDto
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
    protected async Task<AuthResponseDto> LoginUserSuccessfullyAsync(LoginDto loginDto) // Updated to use new LoginDto
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
        var loginDto = new LoginDto // Use new LoginDto
        {
            Email = "admin@jacore.app",
            Password = "AdminPassword123!"
        };

        Console.WriteLine($"---> Attempting to log in as seeded admin: {loginDto.Email}");

        // Login as the pre-seeded admin user
        var loginResult = await LoginUserSuccessfullyAsync(loginDto);

        loginResult.AccessToken.Should().NotBeNullOrWhiteSpace("because admin login should succeed and provide a token.");

        Console.WriteLine($"---> Successfully obtained access token for admin: {loginDto.Email}");
        return loginResult.AccessToken;
    }
}
