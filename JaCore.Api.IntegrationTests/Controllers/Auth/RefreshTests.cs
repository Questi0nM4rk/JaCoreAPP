using FluentAssertions;
using JaCore.Api.DTOs.Auth;
using JaCore.Api.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace JaCore.Api.IntegrationTests.Controllers.Auth;

public class RefreshTests : AuthTestsBase
{
    public RefreshTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Refresh_WithValidTokens_ReturnsOkAndNewTokens()
    {
        // Arrange: Register and Login using success helpers
        var email = $"refresh-ok-{Guid.NewGuid()}@example.com";
        var password = "Password123!";
        await RegisterUserSuccessfullyAsync(email, password);
        var loginResult = await LoginUserSuccessfullyAsync(email, password);
        // loginResult is guaranteed non-null here

        var initialAccessToken = loginResult.AccessToken;
        var initialRefreshToken = loginResult.RefreshToken;

        var refreshRequest = new TokenRefreshRequestDto(initialRefreshToken!);

        // Act: Call refresh endpoint with the initial access token in header and refresh token in body
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", initialAccessToken);
        var refreshResponse = await _client.PostAsJsonAsync(RefreshUrl, refreshRequest);
        _client.DefaultRequestHeaders.Authorization = null; // Clean up header

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        refreshResult.Should().NotBeNull();
        refreshResult!.Succeeded.Should().BeTrue();
        refreshResult.AccessToken.Should().NotBeNullOrWhiteSpace().And.NotBe(initialAccessToken);
        refreshResult.RefreshToken.Should().NotBeNullOrWhiteSpace(); // Could be the same or a new one depending on strategy
        refreshResult.UserId.Should().Be(loginResult.UserId);
        refreshResult.Email.Should().Be(email);
    }

    [Fact]
    public async Task Refresh_WithInvalidRefreshToken_ReturnsUnauthorized()
    {
        // Arrange: Register and Login using success helpers
        var email = $"refresh-invalid-rt-{Guid.NewGuid()}@example.com";
        var password = "Password123!";
        await RegisterUserSuccessfullyAsync(email, password);
        var loginResult = await LoginUserSuccessfullyAsync(email, password);
        // loginResult is guaranteed non-null here

        var validAccessToken = loginResult.AccessToken;
        var invalidRefreshToken = "this-is-not-a-valid-refresh-token";

        var refreshRequest = new TokenRefreshRequestDto(invalidRefreshToken);

        // Act: Call refresh endpoint with valid access token but invalid refresh token
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", validAccessToken);
        var refreshResponse = await _client.PostAsJsonAsync(RefreshUrl, refreshRequest);
        _client.DefaultRequestHeaders.Authorization = null;

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithoutAccessTokenHeader_ReturnsUnauthorized()
    {
        // Arrange: Register and Login using success helpers
        var email = $"refresh-no-at-{Guid.NewGuid()}@example.com";
        var password = "Password123!";
        await RegisterUserSuccessfullyAsync(email, password);
        var loginResult = await LoginUserSuccessfullyAsync(email, password);
        // loginResult is guaranteed non-null here

        var validRefreshToken = loginResult.RefreshToken;
        var refreshRequest = new TokenRefreshRequestDto(validRefreshToken!);

        // Act: Call refresh endpoint without the Authorization header
        _client.DefaultRequestHeaders.Authorization = null;
        var refreshResponse = await _client.PostAsJsonAsync(RefreshUrl, refreshRequest);

        // Assert
        // The API needs the expired Access Token to identify the user for the refresh token lookup
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithMissingRefreshTokenBody_ReturnsBadRequest()
    {
        // Arrange: Register and Login using success helpers
        var email = $"refresh-no-rt-body-{Guid.NewGuid()}@example.com";
        var password = "Password123!";
        await RegisterUserSuccessfullyAsync(email, password);
        var loginResult = await LoginUserSuccessfullyAsync(email, password);
        // loginResult is guaranteed non-null here

        var validAccessToken = loginResult.AccessToken;

        // Act: Call refresh endpoint with valid access token but null body
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", validAccessToken);
        // Sending null or an empty object might depend on how model binding is configured
        var refreshResponse = await _client.PostAsJsonAsync<TokenRefreshRequestDto?>(RefreshUrl, null);
        _client.DefaultRequestHeaders.Authorization = null;

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
