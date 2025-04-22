using FluentAssertions;
using JaCore.Api.Helpers;
using JaCore.Api.IntegrationTests.Controllers.Auth;
using JaCore.Api.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace JaCore.Api.IntegrationTests.Controllers.Users;

[Collection("Database Collection")]
public class GetAllUsersTests : AuthTestsBase
{
    private const string UsersBaseUrl = $"{ApiConstants.ApiRoutePrefix}/users";

    public GetAllUsersTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAllUsers_WithAdminToken_ReturnsOkAndUserList()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterUserDto(
            Email: $"std-user-{Guid.NewGuid()}@example.com",
            FirstName: "Standard",
            LastName: "User",
            Password: "Password123!"
        );
        await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.GetAsync(UsersBaseUrl);
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        users.Should().NotBeNull();
        users.Should().HaveCountGreaterThanOrEqualTo(1);
        users.Should().Contain(u => u.Email == registerDto.Email);
    }

    [Fact]
    public async Task GetAllUsers_WithStandardUserToken_ReturnsForbidden()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterUserDto(
            Email: $"std-user-forbidden-{Guid.NewGuid()}@example.com",
            FirstName: "Standard",
            LastName: "Forbidden",
            Password: "Password123!"
        );
        await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO

        var loginDto = new LoginUserDto(registerDto.Email, registerDto.Password);
        var loginResult = await LoginUserSuccessfullyAsync(loginDto); // Pass DTO
        var standardUserToken = loginResult.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", standardUserToken);
        var response = await _client.GetAsync(UsersBaseUrl);
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllUsers_WithoutToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync(UsersBaseUrl);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private class UserDto
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public List<string>? Roles { get; set; }
    }
}
