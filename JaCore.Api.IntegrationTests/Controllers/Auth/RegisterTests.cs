using FluentAssertions;
using JaCore.Api.IntegrationTests.Helpers; // For Factory, Collection
using System.Net;
using Xunit;

namespace JaCore.Api.IntegrationTests.Controllers.Auth;

public class RegisterTests : AuthTestsBase
{
    // Constructor passes the factory to the base class
    public RegisterTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_WithValidData_ReturnsOkAndTokens()
    {
        // Arrange
        var email = $"register-ok-{Guid.NewGuid()}@example.com";
        var password = "Password123!";

        // Act
        var (result, response) = await RegisterUserAsync(email, password, "Register", "Ok");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Succeeded.Should().BeTrue();
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.Email.Should().Be(email);
        result.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange: Register a user first using the success helper
        var email = $"duplicate-{Guid.NewGuid()}@example.com";
        var password = "Password123!";
        await RegisterUserSuccessfullyAsync(email, password);
        // No need to check firstResponse or firstResult

        // Act: Try to register the same email again using the original helper
        var (secondResult, secondResponse) = await RegisterUserAsync(email, password);

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
        var email = $"invalid-pass-{Guid.NewGuid()}@example.com";
        var invalidPassword = "123"; // Assuming this violates password policy

        // Act
        var (result, response) = await RegisterUserAsync(email, invalidPassword);

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
        email ??= $"missing-data-{Guid.NewGuid()}@example.com";

        // Act
        var (result, response) = await RegisterUserAsync(email, password, firstName, lastName);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().BeNull();
    }
}
