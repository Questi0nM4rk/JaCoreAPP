using FluentAssertions;
using JaCore.Api.Helpers;
using JaCore.Api.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;
using JaCore.Api.IntegrationTests.DTOs.Auth;
using JaCore.Api.IntegrationTests.DTOs.Users;
using JaCore.Api.IntegrationTests.Controllers.Base;


namespace JaCore.Api.IntegrationTests.Controllers.Users;

public class MeTests : AuthTestsBase
{
    public MeTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetMe_WithValidToken_ReturnsUserData()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterDto
        {
            Email = $"me-ok-{Guid.NewGuid()}@example.com",
            FirstName = "MeFirst",
            LastName = "MeLast",
            Password = "Password123!"
        };
        var registerResult = await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto);

        var loginDto = new LoginDto { Email = registerDto.Email, Password = registerDto.Password };
        var loginResult = await LoginUserSuccessfullyAsync(loginDto);

        var accessToken = loginResult.AccessToken;

        // Act: Make request with Authorization header
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var meResponse = await _client.GetAsync(ApiConstants.UserRoutes.Me);
        _client.DefaultRequestHeaders.Authorization = null; // Clean up header

        // Assert
        registerResult.UserId.Should().NotBeNullOrWhiteSpace();
        meResponse.Should().NotBeNull();
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var meResult = await meResponse.Content.ReadFromJsonAsync<UserDto>();
        meResult.Should().NotBeNull();
        meResult.Email.Should().Be(registerDto.Email);
        meResult.FirstName.Should().Be(registerDto.FirstName);
        meResult.LastName.Should().Be(registerDto.LastName);
        meResult.Id.ToString().Should().Be(registerResult.UserId);
        meResult.Roles.Should().NotBeNull().And.Contain("User");
    }

    [Fact]
    public async Task GetMe_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null; // Ensure no token

        // Act
        var meResponse = await _client.GetAsync(ApiConstants.UserRoutes.Me);

        // Assert
        meResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
