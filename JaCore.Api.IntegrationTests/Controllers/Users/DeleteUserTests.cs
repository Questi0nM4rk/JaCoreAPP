using FluentAssertions;
using JaCore.Api.Helpers;
using JaCore.Api.IntegrationTests.Controllers.Auth;
using JaCore.Api.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace JaCore.Api.IntegrationTests.Controllers.Users;

[Collection("Database Collection")]
public class DeleteUserTests : AuthTestsBase
{
    private const string UsersBaseUrl = $"{ApiConstants.ApiRoutePrefix}/users";

    public DeleteUserTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task DeleteUser_AdminDeletingUser_ReturnsNoContent()
    {
        var targetUser = await RegisterUserSuccessfullyAsync();
        var targetUserId = targetUser.UserId;
        var adminAccessToken = await GetAdminAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.DeleteAsync($"{UsersBaseUrl}/{targetUserId}");
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteUser_UserDeletingOtherUser_ReturnsForbidden()
    {
        var targetUser = await RegisterUserSuccessfullyAsync();
        var targetUserId = targetUser.UserId;
        var requesterEmail = $"requester-delete-{Guid.NewGuid()}@example.com";
        var requesterPassword = "Password123!";
        await RegisterUserSuccessfullyAsync(requesterEmail, requesterPassword);
        var requesterLogin = await LoginUserSuccessfullyAsync(requesterEmail, requesterPassword);
        var requesterToken = requesterLogin.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", requesterToken);
        var response = await _client.DeleteAsync($"{UsersBaseUrl}/{targetUserId}");
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteUser_UserNotFound_ReturnsNotFound()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync();
        var nonExistentUserId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.DeleteAsync($"{UsersBaseUrl}/{nonExistentUserId}");
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_WithoutToken_ReturnsUnauthorized()
    {
        var targetUserId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.DeleteAsync($"{UsersBaseUrl}/{targetUserId}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
