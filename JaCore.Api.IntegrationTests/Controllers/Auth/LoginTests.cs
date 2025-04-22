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
        // Arrange: Register a user first using the success helper
        var email = $"login-ok-{Guid.NewGuid()}@example.com";
        var password = "Password123!";
        var registerResult = await RegisterUserSuccessfullyAsync(email, password);
        // No need to check registerResponse or registerResult for null here

        // Act: Still use the original LoginUserAsync as login is the action under test
        var (loginResult, loginResponse) = await LoginUserAsync(email, password);

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        // loginResult null check is now inside LoginUserAsync helper for success cases
        loginResult!.Succeeded.Should().BeTrue();
        loginResult.AccessToken.Should().NotBeNullOrWhiteSpace();
        loginResult.RefreshToken.Should().NotBeNullOrWhiteSpace();
        loginResult.UserId.Should().Be(registerResult.UserId);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange: Register a user first using the success helper
        var email = $"login-fail-pass-{Guid.NewGuid()}@example.com";
        var password = "Password123!";
        await RegisterUserSuccessfullyAsync(email, password);
        // No need to check registerResponse or registerResult

        // Act: Still use the original LoginUserAsync as login failure is the action under test
        var (loginResult, loginResponse) = await LoginUserAsync(email, "WrongPassword!");

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        loginResult.Should().BeNull(); // Expect null result on failure
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var email = $"nonexistent-{Guid.NewGuid()}@example.com";
        var password = "Password123!";

        // Act
        var (loginResult, loginResponse) = await LoginUserAsync(email, password);

        // Assert
        // Depending on implementation, this might be Unauthorized or BadRequest.
        // Unauthorized is common for security reasons (don't reveal if user exists).
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
        // No user registration needed as validation should happen first

        // Act
        var (result, response) = await LoginUserAsync(email, password);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().BeNull();
    }
}
