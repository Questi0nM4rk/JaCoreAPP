using FluentAssertions;
using JaCore.Api.Helpers;
using JaCore.Api.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using JaCore.Api.IntegrationTests.DTOs.Auth;
using JaCore.Api.IntegrationTests.Controllers.Base;

namespace JaCore.Api.IntegrationTests.Controllers.Auth;

public class LoginTests : AuthTestsBase
{
    public LoginTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task LoginAsAdmin_ReturnsOkAndTokens()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        adminAccessToken.Should().NotBeNullOrWhiteSpace(); // Assert that the token is not null or empty
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokensAndUserData()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterDto
        {
            Email = $"login-ok-{Guid.NewGuid()}@example.com",
            FirstName = "Login",
            LastName = "Ok",
            Password = "Password123!"
        };
        var registerResult = await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO

        // Act: Still use the original LoginUserAsync as login is the action under test
        var loginDto = new LoginDto { Email = registerDto.Email, Password = registerDto.Password };
        (AuthResponseDto? loginResult, HttpResponseMessage loginResponse) = await LoginUserAsync(loginDto); // Pass DTO

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        loginResult.Should().NotBeNull(); // Assert not null after checking status code
        loginResult!.Succeeded.Should().BeTrue();
        loginResult.AccessToken.Should().NotBeNullOrWhiteSpace();
        loginResult.RefreshToken.Should().NotBeNullOrWhiteSpace();
        loginResult.UserId.Should().Be(registerResult.UserId);
    }

    [Fact]
    public async Task Login_WithValidCredentials_AfterRegistration_ReturnsTokensAndUserData()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync();
        var registerDto = new RegisterDto
        {
            Email = $"login-ok-{Guid.NewGuid()}@example.com",
            FirstName = "Login",
            LastName = "Ok",
            Password = "Password123!"
        };
        var registerResult = await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto);

        // Act: Still use the original LoginUserAsync as login is the action under test
        var loginDto = new LoginDto { Email = registerDto.Email, Password = registerDto.Password };
        (AuthResponseDto? loginResult, HttpResponseMessage loginResponse) = await LoginUserAsync(loginDto); // Pass DTO

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        loginResult.Should().NotBeNull(); // Assert not null after checking status code
        loginResult!.Succeeded.Should().BeTrue();
        loginResult.AccessToken.Should().NotBeNullOrWhiteSpace();
        loginResult.RefreshToken.Should().NotBeNullOrWhiteSpace();
        loginResult.UserId.Should().Be(registerResult.UserId);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterDto
        {
            Email = $"login-fail-pass-{Guid.NewGuid()}@example.com",
            FirstName = "LoginFail",
            LastName = "Pass",
            Password = "Password123!"
        };
        await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO

        // Act: Still use the original LoginUserAsync as login failure is the action under test
        var loginDto = new LoginDto { Email = registerDto.Email, Password = "WrongPassword!" };
        (AuthResponseDto? loginResult, HttpResponseMessage loginResponse) = await LoginUserAsync(loginDto); // Pass DTO

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        loginResult.Should().BeNull(); // Expect null result on failure
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = $"nonexistent-{Guid.NewGuid()}@example.com",
            Password = "Password123!"
        };

        // Act
        (AuthResponseDto? result, HttpResponseMessage response) = await LoginUserAsync(loginDto); // Pass DTO

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("", "Password123!")] // Missing Email
    [InlineData("test@example.com", "")] // Missing Password
    [InlineData("invalid-email", "Password123!")] // Invalid Email format
    public async Task Login_WithMissingOrInvalidData_ReturnsBadRequest(
        string email, string password)
    {
        // Arrange
        var loginDto = new LoginDto { Email = email, Password = password };

        // Act
        var (result, response) = await LoginUserAsync(loginDto); // Pass DTO

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().BeNull();
    }
}
