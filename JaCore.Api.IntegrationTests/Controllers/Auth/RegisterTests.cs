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

public class RegisterTests : AuthTestsBase
{
    // Constructor passes the factory to the base class
    public RegisterTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_WithValidData_ReturnsTokensAndUserData()
    {
        // Arrange
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token
        var registerDto = new RegisterDto
        {
            Email = $"register-ok-{Guid.NewGuid()}@example.com",
            Password = "Password123!",
            FirstName = "Register",
            LastName = "Ok"
        };

        // Act
        // Use the updated RegisterUserAsync with admin token and DTO
        (AuthResponseDto? result, HttpResponseMessage response) = await RegisterUserAsync(adminAccessToken, registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Succeeded.Should().BeTrue();
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.Email.Should().Be(registerDto.Email);
        result.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange: Get admin token and register a user first
        var adminAccessToken = await GetAdminAccessTokenAsync();
        var registerDto = new RegisterDto
        {
            Email = $"duplicate-{Guid.NewGuid()}@example.com",
            Password = "Password123!",
            FirstName = "Duplicate",
            LastName = "Test"
        };
        // Use the updated RegisterUserSuccessfullyAsync with admin token and DTO
        await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto);

        // Act: Try to register the same email again
        // Use the updated RegisterUserAsync with admin token and DTO
        (AuthResponseDto? secondResult, HttpResponseMessage secondResponse) = await RegisterUserAsync(adminAccessToken, registerDto);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        secondResult.Should().BeNull();
        // Optionally assert specific error details if the API provides them
        // var error = await secondResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        // error?.Detail.Should().Contain("already exists");
    }

    [Fact]
    public async Task Register_WithInvalidPassword_ReturnsBadRequest()
    {
        // Arrange
        var adminAccessToken = await GetAdminAccessTokenAsync();
        var registerDto = new RegisterDto
        {
            Email = $"invalid-pass-{Guid.NewGuid()}@example.com",
            Password = "123", // Assuming this violates password policy
            FirstName = "Invalid",
            LastName = "Password"
        };

        // Act
        // Use the updated RegisterUserAsync with admin token and DTO
        (AuthResponseDto? result, HttpResponseMessage response) = await RegisterUserAsync(adminAccessToken, registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().BeNull();
        // Optionally assert specific error details
        // var error = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        // error?.Detail.Should().Contain("Passwords must have at least");
    }

    [Theory]
    [InlineData("", "LastName", "Password123!")] // Missing FirstName
    [InlineData("FirstName", "", "Password123!")] // Missing LastName
    [InlineData("FirstName", "LastName", "")] // Missing Password
    [InlineData("FirstName", "LastName", "Password123!", "invalid-email")] // Invalid Email format
    public async Task Register_WithMissingOrInvalidData_ReturnsBadRequest(
        string firstName, string lastName, string password, string? email = null)
    {
        // Arrange
        var adminAccessToken = await GetAdminAccessTokenAsync();
        var registerDto = new RegisterDto
        {
            Email = email ?? $"missing-data-{Guid.NewGuid()}@example.com",
            Password = password,
            FirstName = firstName,
            LastName = lastName
        };

        // Act
        // Use the updated RegisterUserAsync with admin token and DTO
        var (result, response) = await RegisterUserAsync(adminAccessToken, registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().BeNull();
    }

    [Fact]
    public async Task Register_WithoutAdminToken_ReturnsUnauthorized()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = $"register-unauth-{Guid.NewGuid()}@example.com",
            Password = "Password123!",
            FirstName = "Register",
            LastName = "Unauth"
        };

        // Act
        // Use the updated RegisterUserAsync with admin token and DTO
        (AuthResponseDto? result, HttpResponseMessage response) = await RegisterUserAsync("", registerDto); // Pass empty token

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.Should().BeNull();
    }
}
