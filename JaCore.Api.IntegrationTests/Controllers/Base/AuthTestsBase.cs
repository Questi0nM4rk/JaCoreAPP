using FluentAssertions;
using JaCore.Api.DTOs.Auth; // Use API DTOs
using JaCore.Api.Helpers;
using JaCore.Api.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;
using JaCore.Api.Data;
using JaCore.Api.Entities.Identity;
using JaCore.Common; // For RoleConstants
using System.Collections.Generic; // For List<string>
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JaCore.Api.IntegrationTests.Controllers.Base;

[Collection("Database Collection")]
public abstract class AuthTestsBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient _client;
    protected readonly CustomWebApplicationFactory _factory;
    protected readonly ILogger<AuthTestsBase> _logger;

    protected AuthTestsBase(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        _logger = factory.Services.GetRequiredService<ILogger<AuthTestsBase>>();
        _logger.LogInformation($"---> AuthTestsBase initialized for {GetType().Name}.");
    }

    // --- Common Helper Methods ---

    protected async Task<(AuthResponseDto? AuthResult, HttpResponseMessage Response)> RegisterUserAsync(
        string adminAccessToken,
        RegisterDto registerDto) // <<< Use API RegisterDto
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
        LoginDto loginDto) // <<< Use API LoginDto
    {
        _logger.LogInformation("Attempting to login user: {Email}", loginDto.Email);
        var response = await _client.PostAsJsonAsync(ApiConstants.AuthRoutes.Login, loginDto);
        AuthResponseDto? result = null;
        if (response.IsSuccessStatusCode)
        {
            try
            {
                result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Failed to deserialize successful login response for {Email}. Status: {StatusCode}", loginDto.Email, response.StatusCode);
            }
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
             _logger.LogWarning("Login attempt failed for {Email}. Status: {StatusCode}. Response: {ErrorContent}",
                 loginDto.Email, response.StatusCode, errorContent);
        }
        _logger.LogInformation("Login attempt for {Email} completed. Status: {StatusCode}", loginDto.Email, response.StatusCode);
        return (result, response);
    }

    protected async Task<(AuthResponseDto? AuthResult, HttpResponseMessage Response)> RefreshTokenAsync(
        string accessToken, // <<< Need expired access token for user identification
        string refreshToken)
    {
        _logger.LogInformation("Attempting token refresh.");
        // Use TokenRefreshRequestDto from API
        var requestDto = new TokenRefreshRequestDto(refreshToken);
        var request = new HttpRequestMessage(HttpMethod.Post, ApiConstants.AuthRoutes.Refresh)
        {
            Content = JsonContent.Create(requestDto)
        };
        // Add the expired access token to the header for user identification
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);
        AuthResponseDto? result = null;
        if (response.IsSuccessStatusCode)
        {
             try
             {
                 result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Failed to deserialize successful refresh response. Status: {StatusCode}", response.StatusCode);
             }
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Token refresh attempt failed. Status: {StatusCode}. Response: {ErrorContent}", response.StatusCode, errorContent);
        }
        _logger.LogInformation("Token refresh attempt completed. Status: {StatusCode}", response.StatusCode);
        return (result, response);
    }

    protected async Task<HttpResponseMessage> LogoutAsync(string accessToken, string refreshToken)
    {
        _logger.LogInformation("Attempting logout.");
        // Use TokenRefreshRequestDto from API
        var requestDto = new TokenRefreshRequestDto(refreshToken);
        var request = new HttpRequestMessage(HttpMethod.Post, ApiConstants.AuthRoutes.Logout)
        {
            Content = JsonContent.Create(requestDto)
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _client.SendAsync(request);
        _logger.LogInformation("Logout attempt completed. Status: {StatusCode}", response.StatusCode);
        return response;
    }

    // --- Success-Oriented Helper Methods ---

    /// <summary>
    /// Registers a user (requires admin privileges) and asserts that the operation was successful.
    /// Throws an exception if registration fails or returns null data.
    /// </summary>
    /// <param name="adminAccessToken">The access token of an authenticated admin user.</param>
    /// <returns>The non-nullable AuthResponseDto on success.</returns>
    protected async Task<AuthResponseDto> RegisterUserSuccessfullyAsync(
        string adminAccessToken,
        RegisterDto registerDto) // <<< Use API RegisterDto
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
    protected async Task<AuthResponseDto> LoginUserSuccessfullyAsync(LoginDto loginDto) // <<< Use API LoginDto
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
        // Use API LoginDto - assuming positional constructor
        var loginDto = new LoginDto(
            Email: "admin@jacore.app",
            Password: "AdminPassword123!"
        );

        Console.WriteLine($"---> Attempting to log in as seeded admin: {loginDto.Email}");

        // Login as the pre-seeded admin user
        var loginResult = await LoginUserSuccessfullyAsync(loginDto);

        loginResult.AccessToken.Should().NotBeNullOrWhiteSpace("because admin login should succeed and provide a token.");

        Console.WriteLine($"---> Successfully obtained access token for admin: {loginDto.Email}");
        return loginResult.AccessToken;
    }

    // Helper to create a standard user for tests
    protected async Task<(AuthResponseDto UserAuth, RegisterDto UserCredentials)> CreateStandardUserAsync(string adminAccessToken)
    {
        // Use property initializers for the nominal record
        var credentials = new RegisterDto
        {
            Email = $"testuser-{Guid.NewGuid()}@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "Password123!",
            ConfirmPassword = "Password123!", // Add required ConfirmPassword
            Roles = new List<string> { RoleConstants.Roles.User }
        };
        var authResult = await RegisterUserSuccessfullyAsync(adminAccessToken, credentials);
        return (authResult, credentials);
    }
}
