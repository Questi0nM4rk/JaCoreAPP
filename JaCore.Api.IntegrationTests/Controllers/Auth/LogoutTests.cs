using FluentAssertions;
using JaCore.Api.Helpers;
using JaCore.Api.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;
using JaCore.Api.IntegrationTests.Controllers.Base;
using JaCore.Api.DTOs.Auth;

namespace JaCore.Api.IntegrationTests.Controllers.Auth;

public class LogoutTests : AuthTestsBase
{
    public LogoutTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Logout_WithValidTokens_ReturnsNoContentAndRevokesToken()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterDto
        {
            Email = $"logout-ok-{Guid.NewGuid()}@example.com",
            FirstName = "Logout",
            LastName = "Ok",
            Password = "Password123!"
        };
        await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO

        var loginDto = new LoginDto { Email = registerDto.Email, Password = registerDto.Password };
        var loginResult = await LoginUserSuccessfullyAsync(loginDto); // Pass DTO
        // loginResult is guaranteed non-null here

        var accessToken = loginResult.AccessToken;
        var refreshTokenToRevoke = loginResult.RefreshToken;

        var logoutRequest = new TokenRefreshRequestDto(refreshTokenToRevoke!);

        // Act: Call logout with valid access token and the refresh token to revoke
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var logoutResponse = await _client.PostAsJsonAsync(ApiConstants.AuthRoutes.Logout, logoutRequest);
        _client.DefaultRequestHeaders.Authorization = null;

        // Assert: Logout successful
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        refreshTokenToRevoke.Should().NotBeNullOrWhiteSpace(); // Ensure the token is not null or empty

        // Assert: Try to refresh using the revoked token (should fail)
        var refreshRequestAfterLogout = new TokenRefreshRequestDto(refreshTokenToRevoke);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken); // Still need AT header for refresh
        var refreshResponseAfterLogout = await _client.PostAsJsonAsync(ApiConstants.AuthRoutes.Refresh, refreshRequestAfterLogout);
        _client.DefaultRequestHeaders.Authorization = null;

        refreshResponseAfterLogout.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithoutAccessToken_ReturnsUnauthorized()
    {
        // Arrange: Need a refresh token, but don't need to be logged in for this specific check
        var refreshToken = "some-refresh-token"; // Doesn't need to be valid for this check
        var logoutRequest = new TokenRefreshRequestDto(refreshToken);

        // Act: Call logout without Authorization header
        _client.DefaultRequestHeaders.Authorization = null;
        var logoutResponse = await _client.PostAsJsonAsync(ApiConstants.AuthRoutes.Logout, logoutRequest);

        // Assert
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithInvalidRefreshTokenInBody_ReturnsNoContent()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterDto
        {
            Email = $"logout-invalid-rt-{Guid.NewGuid()}@example.com",
            FirstName = "LogoutInvalid",
            LastName = "Rt",
            Password = "Password123!"
        };
        await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO

        var loginDto = new LoginDto { Email = registerDto.Email, Password = registerDto.Password };
        var loginResult = await LoginUserSuccessfullyAsync(loginDto); // Pass DTO
        // loginResult is guaranteed non-null here

        var accessToken = loginResult.AccessToken;
        var invalidRefreshToken = "this-is-definitely-not-valid";

        var logoutRequest = new TokenRefreshRequestDto(invalidRefreshToken);

        // Act: Call logout with valid access token but an invalid refresh token
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var logoutResponse = await _client.PostAsJsonAsync(ApiConstants.AuthRoutes.Logout, logoutRequest);
        _client.DefaultRequestHeaders.Authorization = null;

        // Assert
        // The endpoint requires authentication (valid AT), but the goal is to ensure the RT
        // is revoked. If it's already invalid/revoked, the goal is met.
        // So, NoContent is often the correct response.
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_WithMissingRefreshTokenBody_ReturnsBadRequest()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterDto
        {
            Email = $"logout-no-rt-body-{Guid.NewGuid()}@example.com",
            FirstName = "LogoutNoRt",
            LastName = "Body",
            Password = "Password123!"
        };
        await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO

        var loginDto = new LoginDto { Email = registerDto.Email, Password = registerDto.Password };
        var loginResult = await LoginUserSuccessfullyAsync(loginDto); // Pass DTO
        // loginResult is guaranteed non-null here

        var accessToken = loginResult.AccessToken;

        // Act: Call logout with valid access token but null body
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var logoutResponse = await _client.PostAsJsonAsync<TokenRefreshRequestDto?>(ApiConstants.AuthRoutes.Logout, null);
        _client.DefaultRequestHeaders.Authorization = null;

        // Assert
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
