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
public class UpdateUserRolesTests : AuthTestsBase
{
    private const string UsersBaseUrl = $"{ApiConstants.ApiRoutePrefix}/users";

    public UpdateUserRolesTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task UpdateUserRoles_AdminUpdatesRoles_ReturnsNoContent()
    {
        var targetUser = await RegisterUserSuccessfullyAsync();
        var targetUserId = targetUser.UserId;
        var adminAccessToken = await GetAdminAccessTokenAsync();
        var updateRolesDto = new UpdateUserRolesDto { Roles = new List<string> { "Admin" } };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.PutAsJsonAsync($"{UsersBaseUrl}/{targetUserId}/roles", updateRolesDto);
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateUserRoles_StandardUserForbidden()
    {
        var targetUser = await RegisterUserSuccessfullyAsync();
        var targetUserId = targetUser.UserId;
        var requesterEmail = $"requester-roles-{Guid.NewGuid()}@example.com";
        var requesterPassword = "Password123!";
        await RegisterUserSuccessfullyAsync(requesterEmail, requesterPassword);
        var requesterLogin = await LoginUserSuccessfullyAsync(requesterEmail, requesterPassword);
        var requesterToken = requesterLogin.AccessToken;
        var updateRolesDto = new UpdateUserRolesDto { Roles = new List<string> { "Admin" } };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", requesterToken);
        var response = await _client.PutAsJsonAsync($"{UsersBaseUrl}/{targetUserId}/roles", updateRolesDto);
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateUserRoles_UserNotFound_ReturnsNotFound()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync();
        var nonExistentUserId = Guid.NewGuid();
        var updateRolesDto = new UpdateUserRolesDto { Roles = new List<string> { "Admin" } };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.PutAsJsonAsync($"{UsersBaseUrl}/{nonExistentUserId}/roles", updateRolesDto);
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUserRoles_WithoutToken_ReturnsUnauthorized()
    {
        var targetUserId = Guid.NewGuid();
        var updateRolesDto = new UpdateUserRolesDto { Roles = new List<string> { "Admin" } };
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PutAsJsonAsync($"{UsersBaseUrl}/{targetUserId}/roles", updateRolesDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private class UpdateUserRolesDto
    {
        public List<string>? Roles { get; set; }
    }
}
