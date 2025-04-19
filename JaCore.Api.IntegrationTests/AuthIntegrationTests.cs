using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JaCore.Api.Dtos.User; // Assuming DTO namespace
using JaCore.Api.Models.User; // For ApplicationUser if needed
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace JaCore.Api.IntegrationTests;

public class AuthIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true }; // Match API config

    public AuthIntegrationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(); // Create a default client instance
    }

    // --- Helper Methods ---
    private async Task<AuthResponseDto?> LoginAsync(string email, string password)
    {
        var loginDto = new UserLoginDto { Email = email, Password = password };
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
        if (!response.IsSuccessStatusCode)
        {
            return null; // Or throw an exception if login failure should halt the test
        }
        return await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
    }

    private async Task<HttpClient> GetAuthenticatedClientAsync(string email = ApiWebApplicationFactory.AdminEmail, 
                                                             string password = ApiWebApplicationFactory.DefaultPassword)
    {
        var authResponse = await LoginAsync(email, password);
        if (authResponse == null || !authResponse.IsSuccess || string.IsNullOrEmpty(authResponse.Token))
        {
            throw new Exception($"Failed to log in as {email} for authenticated client setup.");
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);
        return client;
    }

    // Helper to register a user programmatically (useful for setting up specific test states)
    private async Task<(string UserId, string Email, string Password)> RegisterUserAsync(string role, bool isActive = true)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var email = $"{role.ToLower()}.{Guid.NewGuid().ToString().Substring(0, 6)}@test.com";
        var password = ApiWebApplicationFactory.DefaultPassword; // Use default for simplicity
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = role,
            LastName = "TestUser",
            IsActive = isActive
        };
        
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new Exception($"Test setup failed: Could not create user. Errors: {string.Join(",", result.Errors.Select(e => e.Description))}");
        
        result = await userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
            throw new Exception($"Test setup failed: Could not add user to role {role}. Errors: {string.Join(",", result.Errors.Select(e => e.Description))}");

        return (user.Id, email, password);
    }

    // --- Registration Tests ---

    [Fact]
    public async Task Register_Unauthenticated_ReturnsForbidden()
    {
        // Arrange
        var registerDto = new UserRegistrationDto
        {
            Email = "unauth.register@test.com", Password = "Pass1!", FirstName = "Unauth", LastName = "Test", Role = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Register_AdminAuthenticated_CreatesUser_ReturnsCreated()
    {
        // Arrange
        var adminClient = await GetAuthenticatedClientAsync(); // Login as default admin
        var newUserEmail = "new.user.register@test.com";
        var registerDto = new UserRegistrationDto
        {
            Email = newUserEmail, Password = "GoodPass123!", FirstName = "New", LastName = "UserReg", Role = "User"
        };

        // Act
        var response = await adminClient.PostAsJsonAsync("/api/Auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var userResponse = await response.Content.ReadFromJsonAsync<UserResponseDto>(_jsonOptions);
        Assert.NotNull(userResponse);
        Assert.Equal(newUserEmail, userResponse.Email);
        Assert.Equal("User", userResponse.Role);
        Assert.True(userResponse.IsActive);

        // Verify Location header (optional but good practice)
        Assert.NotNull(response.Headers.Location);
    }
    
    [Fact]
    public async Task Register_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange
        var adminClient = await GetAuthenticatedClientAsync();
        var registerDto = new UserRegistrationDto // Use admin email which already exists
        { 
            Email = ApiWebApplicationFactory.AdminEmail, Password = "AnotherPass1!", FirstName = "Duplicate", LastName = "Admin", Role = "Admin"
        };

        // Act
        var response = await adminClient.PostAsJsonAsync("/api/Auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        // Optionally check for specific error message in the response body if API provides it
    }

    [Fact]
    public async Task Register_WithInvalidRole_ReturnsBadRequest()
    {
        // Arrange
        var adminClient = await GetAuthenticatedClientAsync();
        var registerDto = new UserRegistrationDto
        {
            Email = "invalid.role@test.com", Password = "Pass1!", FirstName = "Invalid", LastName = "RoleUser", Role = "NonExistentRole"
        };

        // Act
        var response = await adminClient.PostAsJsonAsync("/api/Auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        // Optionally check for specific error message related to the role
    }

    // --- Login Tests ---

    [Fact]
    public async Task Login_SeededAdminUser_ValidCredentials_ReturnsOkAndToken()
    {
        // Arrange
        // User is seeded by the factory

        // Act
        var authResponse = await LoginAsync(ApiWebApplicationFactory.AdminEmail, ApiWebApplicationFactory.DefaultPassword);

        // Assert
        Assert.NotNull(authResponse);
        Assert.True(authResponse.IsSuccess);
        Assert.False(string.IsNullOrEmpty(authResponse.Token));
        Assert.False(string.IsNullOrEmpty(authResponse.RefreshToken));
        Assert.NotNull(authResponse.User);
        Assert.Equal(ApiWebApplicationFactory.AdminEmail, authResponse.User.Email);
        Assert.Equal("Admin", authResponse.User.Role);
    }

    [Fact]
    public async Task Login_SeededAdminUser_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new UserLoginDto
        {
            Email = ApiWebApplicationFactory.AdminEmail,
            Password = "WrongPassword!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        Assert.NotNull(authResponse);
        Assert.False(authResponse.IsSuccess);
        Assert.Contains("Invalid credentials", authResponse.ErrorMessage);
    }

    [Fact]
    public async Task Login_NonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new UserLoginDto { Email = "noone@test.com", Password = "anypass" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task Login_DeactivatedUser_ReturnsUnauthorized()
    {
        // Arrange: Create a deactivated user
        var (_, email, password) = await RegisterUserAsync("User", isActive: false);
        var loginDto = new UserLoginDto { Email = email, Password = password };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        Assert.NotNull(authResponse);
        Assert.False(authResponse.IsSuccess);
        Assert.Contains("Account is deactivated", authResponse.ErrorMessage);
    }

    // --- Refresh Token Tests ---

    [Fact]
    public async Task RefreshToken_ValidTokens_ReturnsNewTokens()
    {
        // Arrange: Log in to get valid tokens
        var initialAuth = await LoginAsync(ApiWebApplicationFactory.AdminEmail, ApiWebApplicationFactory.DefaultPassword);
        Assert.NotNull(initialAuth);
        Assert.True(initialAuth.IsSuccess);
        var initialTokenModel = new TokenModel { AccessToken = initialAuth.Token, RefreshToken = initialAuth.RefreshToken };

        // Act: Wait a moment to ensure the new token isn't identical due to timing
        await Task.Delay(100); 
        var response = await _client.PostAsJsonAsync("/api/Auth/refresh-token", initialTokenModel);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var newAuth = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        Assert.NotNull(newAuth);
        Assert.True(newAuth.IsSuccess);
        Assert.False(string.IsNullOrEmpty(newAuth.Token));
        Assert.False(string.IsNullOrEmpty(newAuth.RefreshToken));
        Assert.NotEqual(initialAuth.Token, newAuth.Token); // Ensure token is actually new
        // Refresh token might or might not be rotated depending on implementation, so don't assert inequality by default
        Assert.NotNull(newAuth.User);
        Assert.Equal(ApiWebApplicationFactory.AdminEmail, newAuth.User.Email);
    }
    
    [Fact]
    public async Task RefreshToken_InvalidRefreshToken_ReturnsUnauthorized()
    {
        // Arrange: Log in, but use a bad refresh token
        var initialAuth = await LoginAsync(ApiWebApplicationFactory.AdminEmail, ApiWebApplicationFactory.DefaultPassword);
        Assert.NotNull(initialAuth);
        var tokenModel = new TokenModel { AccessToken = initialAuth.Token, RefreshToken = "invalid-refresh-token" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/refresh-token", tokenModel);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode); // Expect Unauthorized if refresh token is invalid/not found
    }

    // Test for expired refresh token would require manipulating time or user token expiry in DB - more complex.

    // --- Revoke Tests ---

    [Fact]
    public async Task Revoke_AuthenticatedUser_ClearsRefreshToken_ReturnsNoContent()
    {
        // Arrange: Create user, log in, get client
        var (_, email, password) = await RegisterUserAsync("User");
        var userClient = await GetAuthenticatedClientAsync(email, password);

        // Act
        var response = await userClient.PostAsync("/api/Auth/revoke", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify token is actually revoked (check DB or attempt refresh)
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.Null(user.RefreshToken); // Check if refresh token is cleared
    }
    
    [Fact]
    public async Task Revoke_Unauthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.PostAsync("/api/Auth/revoke", null); // Use unauthenticated client

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task RevokeAll_AdminAuthenticated_ReturnsNoContent()
    {
        // Arrange
        var adminClient = await GetAuthenticatedClientAsync();

        // Act
        var response = await adminClient.PostAsync("/api/Auth/revoke-all", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        // Verification would involve checking multiple users in the DB if possible
    }
    
    [Fact]
    public async Task RevokeAll_NonAdminAuthenticated_ReturnsForbidden()
    {
        // Arrange: Register and login as non-admin
        var (_, email, password) = await RegisterUserAsync("User");
        var userClient = await GetAuthenticatedClientAsync(email, password);

        // Act
        var response = await userClient.PostAsync("/api/Auth/revoke-all", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_InvalidModel_ReturnsBadRequest()
    {
        var tokenModel = new TokenModel { AccessToken = string.Empty, RefreshToken = string.Empty }; // Use string.Empty instead of null
        var response = await _client.PostAsJsonAsync("/api/Auth/refresh-token", tokenModel);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task RefreshToken_InvalidAccessToken_ReturnsBadRequest()
    { 
        var tokenModel = new TokenModel { AccessToken = "invalid.token", RefreshToken = "valid-refresh" };
        var response = await _client.PostAsJsonAsync("/api/Auth/refresh-token", tokenModel);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
} 