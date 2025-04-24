using FluentAssertions;
using JaCore.Api.Helpers;
using JaCore.Api.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using JaCore.Api.IntegrationTests.DTOs.Auth;
using JaCore.Api.IntegrationTests.Controllers.Base;
using System.Net.Http.Headers;

namespace JaCore.Api.IntegrationTests.Controllers.Auth;

public class RefreshTests : AuthTestsBase
{
    public RefreshTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Refresh_WithValidTokens_ReturnsOkAndNewTokens()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterDto
        {
            Email = $"refresh-ok-{Guid.NewGuid()}@example.com",
            FirstName = "Refresh",
            LastName = "Ok",
            Password = "Password123!"
        };
        await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO

        var loginDto = new LoginDto { Email = registerDto.Email, Password = registerDto.Password };
        var loginResult = await LoginUserSuccessfullyAsync(loginDto); // Pass DTO
        // loginResult is guaranteed non-null here

        var initialAccessToken = loginResult.AccessToken;
        var initialRefreshToken = loginResult.RefreshToken;

        var refreshRequest = new TokenRefreshRequestDto(initialRefreshToken!);

        // Act: Call refresh endpoint with the initial access token in header and refresh token in body
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", initialAccessToken);
        var refreshResponse = await _client.PostAsJsonAsync(ApiConstants.AuthRoutes.Refresh, refreshRequest);
        _client.DefaultRequestHeaders.Authorization = null; // Clean up header

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        refreshResult.Should().NotBeNull();
        refreshResult!.Succeeded.Should().BeTrue();
        refreshResult.AccessToken.Should().NotBeNullOrWhiteSpace().And.NotBe(initialAccessToken);
        refreshResult.RefreshToken.Should().NotBeNullOrWhiteSpace(); // Could be the same or a new one depending on strategy
        refreshResult.UserId.Should().Be(loginResult.UserId);
        refreshResult.Email.Should().Be(registerDto.Email);
    }

    [Fact]
    public async Task Refresh_WithInvalidRefreshToken_ReturnsUnauthorized()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterDto
        {
            Email = $"refresh-invalid-rt-{Guid.NewGuid()}@example.com",
            FirstName = "RefreshInvalid",
            LastName = "Rt",
            Password = "Password123!"
        };
        await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO

        var loginDto = new LoginDto { Email = registerDto.Email, Password = registerDto.Password };
        var loginResult = await LoginUserSuccessfullyAsync(loginDto); // Pass DTO
        // loginResult is guaranteed non-null here

        var validAccessToken = loginResult.AccessToken;
        var invalidRefreshToken = "this-is-not-a-valid-refresh-token";

        var refreshRequest = new TokenRefreshRequestDto(invalidRefreshToken);

        // Act: Call refresh endpoint with valid access token but invalid refresh token
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", validAccessToken);
        var refreshResponse = await _client.PostAsJsonAsync(ApiConstants.AuthRoutes.Refresh, refreshRequest);
        _client.DefaultRequestHeaders.Authorization = null;

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithoutAccessTokenHeader_ReturnsUnauthorized()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterDto
        {
            Email = $"refresh-no-at-{Guid.NewGuid()}@example.com",
            FirstName = "RefreshNo",
            LastName = "At",
            Password = "Password123!"
        };
        await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO

        var loginDto = new LoginDto { Email = registerDto.Email, Password = registerDto.Password };
        var loginResult = await LoginUserSuccessfullyAsync(loginDto); // Pass DTO
        // loginResult is guaranteed non-null here

        var validRefreshToken = loginResult.RefreshToken;
        var refreshRequest = new TokenRefreshRequestDto(validRefreshToken!);

        // Act: Call refresh endpoint without the Authorization header
        _client.DefaultRequestHeaders.Authorization = null;
        var refreshResponse = await _client.PostAsJsonAsync(ApiConstants.AuthRoutes.Refresh, refreshRequest);

        // Assert
        // The API needs the expired Access Token to identify the user for the refresh token lookup
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithMissingRefreshTokenBody_ReturnsBadRequest()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterDto
        {
            Email = $"refresh-no-rt-body-{Guid.NewGuid()}@example.com",
            FirstName = "RefreshNoRt",
            LastName = "Body",
            Password = "Password123!"
        };
        await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO

        var loginDto = new LoginDto { Email = registerDto.Email, Password = registerDto.Password };
        var loginResult = await LoginUserSuccessfullyAsync(loginDto); // Pass DTO
        // loginResult is guaranteed non-null here

        var validAccessToken = loginResult.AccessToken;

        // Act: Call refresh endpoint with valid access token but null body
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", validAccessToken);
        // Sending null or an empty object might depend on how model binding is configured
        var refreshResponse = await _client.PostAsJsonAsync<TokenRefreshRequestDto?>(ApiConstants.AuthRoutes.Refresh, null);
        _client.DefaultRequestHeaders.Authorization = null;

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
