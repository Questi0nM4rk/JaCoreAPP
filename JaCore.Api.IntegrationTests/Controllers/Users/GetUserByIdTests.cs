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
        var targetUser = await RegisterUserSuccessfullyAsync(firstName: "Target", lastName: "UserGet");
        var targetUserId = targetUser.UserId;
        targetUserId.Should().NotBeNullOrEmpty();
        var adminAccessToken = await GetAdminAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.GetAsync($"{UsersBaseUrl}/{targetUserId}");
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Id.Should().Be(targetUserId.ToString());
        user.Email.Should().Be(targetUser.Email);
        user.FirstName.Should().Be("Target");
    }

    [Fact]
    public async Task GetUserById_UserGettingSelf_ReturnsOkAndUserData()
    {
        var userEmail = $"self-get-{Guid.NewGuid()}@example.com";
        var userPassword = "Password123!";
        var registerResult = await RegisterUserSuccessfullyAsync(userEmail, userPassword, firstName: "Self", lastName: "Get");
        var loginResult = await LoginUserSuccessfullyAsync(userEmail, userPassword);
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
        user.Email.Should().Be(userEmail);
        user.FirstName.Should().Be("Self");
    }

    [Fact]
    public async Task GetUserById_UserGettingOtherUser_ReturnsForbidden()
    {
        var targetUser = await RegisterUserSuccessfullyAsync(firstName: "TargetOther");
        var targetUserId = targetUser.UserId;
        var requesterEmail = $"requester-other-{Guid.NewGuid()}@example.com";
        var requesterPassword = "Password123!";
        await RegisterUserSuccessfullyAsync(requesterEmail, requesterPassword);
        var requesterLogin = await LoginUserSuccessfullyAsync(requesterEmail, requesterPassword);
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
