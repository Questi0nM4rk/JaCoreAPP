using FluentAssertions;
using JaCore.Api.Helpers;
using JaCore.Api.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;
using JaCore.Api.IntegrationTests.Controllers.Auth;
using JaCore.Api.IntegrationTests.Controllers.Base; // Add this for AuthTestsBase

namespace JaCore.Api.IntegrationTests.Controllers.Auth;

public class AdminOnlyTests : AuthTestsBase // Ensure base class is found
{
    public AdminOnlyTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task AdminOnly_WithAdminUserToken_ReturnsOk()
    {
        // Arrange: Get an access token for a user in the Admin role
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Now calls the base class method

        // Act: Call the admin-only endpoint with the admin token
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.GetAsync(ApiConstants.AuthRoutes.AdminOnly);
        _client.DefaultRequestHeaders.Authorization = null;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Optionally check response content
        // var content = await response.Content.ReadFromJsonAsync<object>();
        // content.Should().NotBeNull();
    }

    [Fact]
    public async Task AdminOnly_WithStandardUserToken_ReturnsForbidden()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterDto
        {
            Email = $"standard-user-{Guid.NewGuid()}@example.com",
            FirstName = "Standard",
            LastName = "User",
            Password = "Password123!"
        };
        await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO

        var loginDto = new LoginDto { Email = registerDto.Email, Password = registerDto.Password };
        var loginResult = await LoginUserSuccessfullyAsync(loginDto); // Pass DTO
        // loginResult is guaranteed non-null here
        var standardUserAccessToken = loginResult.AccessToken;

        // Act: Call the admin-only endpoint with the standard user's token
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", standardUserAccessToken);
        var response = await _client.GetAsync(ApiConstants.AuthRoutes.AdminOnly);
        _client.DefaultRequestHeaders.Authorization = null;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminOnly_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync(ApiConstants.AuthRoutes.AdminOnly);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
