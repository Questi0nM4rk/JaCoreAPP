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
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterUserDto(
            Email: $"target-roles-{Guid.NewGuid()}@example.com",
            FirstName: "Target",
            LastName: "Roles",
            Password: "Password123!"
        );
        var targetUser = await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO
        var targetUserId = targetUser.UserId;
        var updateRolesDto = new UpdateUserRolesDto { Roles = new List<string> { "Admin" } };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.PutAsJsonAsync($"{UsersBaseUrl}/{targetUserId}/roles", updateRolesDto);
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateUserRoles_StandardUserForbidden()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var targetRegisterDto = new RegisterUserDto(
            Email: $"target-forbidden-roles-{Guid.NewGuid()}@example.com",
            FirstName: "Target",
            LastName: "ForbiddenRoles",
            Password: "Password123!"
        );
        var targetUser = await RegisterUserSuccessfullyAsync(adminAccessToken, targetRegisterDto); // Pass token and DTO
        var targetUserId = targetUser.UserId;

        var requesterRegisterDto = new RegisterUserDto(
            Email: $"requester-roles-{Guid.NewGuid()}@example.com",
            FirstName: "Requester",
            LastName: "Roles",
            Password: "Password123!"
        );
        await RegisterUserSuccessfullyAsync(adminAccessToken, requesterRegisterDto); // Pass token and DTO

        var requesterLoginDto = new LoginUserDto(requesterRegisterDto.Email, requesterRegisterDto.Password);
        var requesterLogin = await LoginUserSuccessfullyAsync(requesterLoginDto); // Pass DTO
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
