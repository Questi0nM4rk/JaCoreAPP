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
public class GetUserByIdTests : AuthTestsBase
{
    private const string UsersBaseUrl = $"{ApiConstants.ApiRoutePrefix}/users";

    public GetUserByIdTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetUserById_AdminGettingAnyUser_ReturnsOkAndUserData()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterUserDto(
            Email: $"target-get-{Guid.NewGuid()}@example.com",
            FirstName: "Target",
            LastName: "UserGet",
            Password: "Password123!"
        );
        var targetUser = await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO
        var targetUserId = targetUser.UserId;
        targetUserId.Should().NotBeNullOrEmpty();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.GetAsync($"{UsersBaseUrl}/{targetUserId}");
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Id.Should().Be(targetUserId.ToString());
        user.Email.Should().Be(registerDto.Email);
        user.FirstName.Should().Be(registerDto.FirstName);
    }

    [Fact]
    public async Task GetUserById_UserGettingSelf_ReturnsOkAndUserData()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterUserDto(
            Email: $"self-get-{Guid.NewGuid()}@example.com",
            FirstName: "Self",
            LastName: "Get",
            Password: "Password123!"
        );
        var registerResult = await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO

        var loginDto = new LoginUserDto(registerDto.Email, registerDto.Password);
        var loginResult = await LoginUserSuccessfullyAsync(loginDto); // Pass DTO
        var userId = registerResult.UserId;
        var userToken = loginResult.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var response = await _client.GetAsync($"{UsersBaseUrl}/{userId}");
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        userId.Should().NotBeNullOrEmpty();
        user.Id.Should().Be(userId.ToString());
        user.Email.Should().Be(registerDto.Email);
        user.FirstName.Should().Be(registerDto.FirstName);
    }

    [Fact]
    public async Task GetUserById_UserGettingOtherUser_ReturnsForbidden()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var targetRegisterDto = new RegisterUserDto(
            Email: $"target-other-get-{Guid.NewGuid()}@example.com",
            FirstName: "TargetOther",
            LastName: "Get",
            Password: "Password123!"
        );
        var targetUser = await RegisterUserSuccessfullyAsync(adminAccessToken, targetRegisterDto); // Pass token and DTO
        var targetUserId = targetUser.UserId;

        var requesterRegisterDto = new RegisterUserDto(
            Email: $"requester-other-{Guid.NewGuid()}@example.com",
            FirstName: "Requester",
            LastName: "Other",
            Password: "Password123!"
        );
        await RegisterUserSuccessfullyAsync(adminAccessToken, requesterRegisterDto); // Pass token and DTO

        var requesterLoginDto = new LoginUserDto(requesterRegisterDto.Email, requesterRegisterDto.Password);
        var requesterLogin = await LoginUserSuccessfullyAsync(requesterLoginDto); // Pass DTO
        var requesterToken = requesterLogin.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", requesterToken);
        var response = await _client.GetAsync($"{UsersBaseUrl}/{targetUserId}");
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserById_UserNotFound_ReturnsNotFound()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync();
        var nonExistentUserId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.GetAsync($"{UsersBaseUrl}/{nonExistentUserId}");
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserById_WithoutToken_ReturnsUnauthorized()
    {
        var targetUserId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync($"{UsersBaseUrl}/{targetUserId}");
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
