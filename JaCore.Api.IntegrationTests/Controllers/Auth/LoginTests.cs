using FluentAssertions;
using JaCore.Api.IntegrationTests.Helpers; // For Factory, Collection
using System.Net;
using Xunit;

namespace JaCore.Api.IntegrationTests.Controllers.Auth;

public class LoginTests : AuthTestsBase
{
    public LoginTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkAndTokens()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterUserDto(
            Email: $"login-ok-{Guid.NewGuid()}@example.com",
            FirstName: "Login",
            LastName: "Ok",
            Password: "Password123!"
        );
        var registerResult = await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO

        // Act: Still use the original LoginUserAsync as login is the action under test
        var loginDto = new LoginUserDto(registerDto.Email, registerDto.Password);
        var (loginResult, loginResponse) = await LoginUserAsync(loginDto); // Pass DTO

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
        var registerDto = new RegisterUserDto(
            Email: $"login-fail-pass-{Guid.NewGuid()}@example.com",
            FirstName: "LoginFail",
            LastName: "Pass",
            Password: "Password123!"
        );
        await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO

        // Act: Still use the original LoginUserAsync as login failure is the action under test
        var loginDto = new LoginUserDto(registerDto.Email, "WrongPassword!");
        var (loginResult, loginResponse) = await LoginUserAsync(loginDto); // Pass DTO

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        loginResult.Should().BeNull(); // Expect null result on failure
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginUserDto(
            Email: $"nonexistent-{Guid.NewGuid()}@example.com",
            Password: "Password123!"
        );

        // Act
        var (loginResult, loginResponse) = await LoginUserAsync(loginDto); // Pass DTO

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        loginResult.Should().BeNull();
    }

    [Theory]
    [InlineData("", "Password123!")] // Missing Email
    [InlineData("test@example.com", "")] // Missing Password
    [InlineData("invalid-email", "Password123!")] // Invalid Email format
    public async Task Login_WithMissingOrInvalidData_ReturnsBadRequest(
        string email, string password)
    {
        // Arrange
        var loginDto = new LoginUserDto(email, password);

        // Act
        var (result, response) = await LoginUserAsync(loginDto); // Pass DTO

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().BeNull();
    }
}
