using FluentAssertions;
using JaCore.Api.IntegrationTests.Helpers; // For Factory, Collection
using System.Net;
using Xunit;
// Add using for the DTOs if not already present in AuthTestsBase or globally
// using static JaCore.Api.IntegrationTests.Controllers.Auth.AuthTestsBase; 

namespace JaCore.Api.IntegrationTests.Controllers.Auth;

public class RegisterTests : AuthTestsBase
{
    // Constructor passes the factory to the base class
    public RegisterTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_WithValidData_ReturnsOkAndTokens()
    {
        // Arrange
        var adminToken = await GetAdminAccessTokenAsync(); // Get admin token
        var registerDto = new RegisterUserDto(
            Email: $"register-ok-{Guid.NewGuid()}@example.com",
            Password: "Password123!",
            FirstName: "Register",
            LastName: "Ok"
        );

        // Act
        // Use the updated RegisterUserAsync with admin token and DTO
        var (result, response) = await RegisterUserAsync(adminToken, registerDto);

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
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange: Get admin token and register a user first
        var adminToken = await GetAdminAccessTokenAsync();
        var registerDto = new RegisterUserDto(
            Email: $"duplicate-{Guid.NewGuid()}@example.com",
            Password: "Password123!",
            FirstName: "Duplicate",
            LastName: "Test"
        );
        // Use the updated RegisterUserSuccessfullyAsync with admin token and DTO
        await RegisterUserSuccessfullyAsync(adminToken, registerDto);

        // Act: Try to register the same email again
        // Use the updated RegisterUserAsync with admin token and DTO
        var (secondResult, secondResponse) = await RegisterUserAsync(adminToken, registerDto);

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
        var adminToken = await GetAdminAccessTokenAsync();
        var registerDto = new RegisterUserDto(
            Email: $"invalid-pass-{Guid.NewGuid()}@example.com",
            Password: "123", // Assuming this violates password policy
            FirstName: "Invalid",
            LastName: "Password"
        );

        // Act
        // Use the updated RegisterUserAsync with admin token and DTO
        var (result, response) = await RegisterUserAsync(adminToken, registerDto);

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
        var adminToken = await GetAdminAccessTokenAsync();
        var registerDto = new RegisterUserDto(
            Email: email ?? $"missing-data-{Guid.NewGuid()}@example.com",
            Password: password,
            FirstName: firstName,
            LastName: lastName
        );

        // Act
        // Use the updated RegisterUserAsync with admin token and DTO
        var (result, response) = await RegisterUserAsync(adminToken, registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().BeNull();
    }
}
