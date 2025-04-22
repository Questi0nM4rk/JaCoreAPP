using FluentAssertions;
using JaCore.Api.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace JaCore.Api.IntegrationTests.Controllers.Auth;

public class MeTests : AuthTestsBase
{
    public MeTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetMe_WithValidToken_ReturnsUserData()
    {
        // Arrange: Register and Login using success helpers
        var email = $"me-ok-{Guid.NewGuid()}@example.com";
        var password = "Password123!";
        var firstName = "MeFirst";
        var lastName = "MeLast";
        var registerResult = await RegisterUserSuccessfullyAsync(email, password, firstName, lastName);
        var loginResult = await LoginUserSuccessfullyAsync(email, password);
        // registerResult and loginResult are guaranteed non-null here

        var accessToken = loginResult.AccessToken;

        // Act: Make request with Authorization header
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var meResponse = await _client.GetAsync(MeUrl);
        _client.DefaultRequestHeaders.Authorization = null; // Clean up header

        // Assert
        registerResult.UserId.Should().NotBeNullOrWhiteSpace();
        meResponse.Should().NotBeNull();
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var meResult = await meResponse.Content.ReadFromJsonAsync<MeResponseDto>();
        meResult.Should().NotBeNull();
        meResult!.Email.Should().Be(email);
        meResult.FirstName.Should().Be(firstName);
        meResult.LastName.Should().Be(lastName);
        meResult.UserId.Should().Be(registerResult.UserId.ToString());
        meResult.Roles.Should().NotBeNull().And.Contain("User");
    }

    [Fact]
    public async Task GetMe_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null; // Ensure no token

        // Act
        var meResponse = await _client.GetAsync(MeUrl);

        // Assert
        meResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- Helper DTO for Deserialization ---
    // Adjust properties based on the actual structure returned by your /api/v1/auth/me endpoint
    private class MeResponseDto
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public List<string>? Roles { get; set; }
    }
}
