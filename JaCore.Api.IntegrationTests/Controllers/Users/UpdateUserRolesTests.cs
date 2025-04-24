using FluentAssertions;
using JaCore.Api.Helpers;
using JaCore.Api.IntegrationTests.Controllers.Auth;
using JaCore.Api.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;
using JaCore.Api.IntegrationTests.DTOs.Auth;
using JaCore.Api.IntegrationTests.DTOs.Users;
using JaCore.Api.IntegrationTests.Controllers.Base;

namespace JaCore.Api.IntegrationTests.Controllers.Users;

[Collection("Database Collection")]
public class UpdateUserRolesTests : AuthTestsBase
{
    public UpdateUserRolesTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task UpdateUserRoles_AdminUpdatesRoles_ReturnsNoContent()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterDto
        {
            Email = $"target-roles-{Guid.NewGuid()}@example.com",
            FirstName = "Target",
            LastName = "Roles",
            Password = "Password123!"
        };
        var targetUser = await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO
        var targetUserId = targetUser.UserId;
        var updateRolesDto = new UpdateUserRolesDto { Roles = new List<string> { "Admin" } };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.PutAsJsonAsync($"{ApiConstants.BasePaths.Users}/{targetUserId}/roles", updateRolesDto);
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify roles were updated by fetching the user again
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var getUserResponse = await _client.GetAsync($"{ApiConstants.BasePaths.Users}/{targetUserId}");
        _client.DefaultRequestHeaders.Authorization = null;

        getUserResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var userAfterUpdate = await getUserResponse.Content.ReadFromJsonAsync<UserDto>();
        userAfterUpdate.Should().NotBeNull();
        // Assuming UserDto has a Roles property (adjust if needed)
        userAfterUpdate!.Roles.Should().NotBeNull();
        userAfterUpdate.Roles.Should().BeEquivalentTo(updateRolesDto.Roles);
    }

    [Fact]
    public async Task UpdateUserRoles_StandardUserForbidden()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var targetRegisterDto = new RegisterDto
        {
            Email = $"target-forbidden-roles-{Guid.NewGuid()}@example.com",
            FirstName = "Target",
            LastName = "ForbiddenRoles",
            Password = "Password123!"
        };
        var targetUser = await RegisterUserSuccessfullyAsync(adminAccessToken, targetRegisterDto); // Pass token and DTO
        var targetUserId = targetUser.UserId;

        var requesterRegisterDto = new RegisterDto
        {
            Email = $"requester-roles-{Guid.NewGuid()}@example.com",
            FirstName = "Requester",
            LastName = "Roles",
            Password = "Password123!"
        };
        await RegisterUserSuccessfullyAsync(adminAccessToken, requesterRegisterDto); // Pass token and DTO

        var requesterLoginDto = new LoginDto { Email = requesterRegisterDto.Email, Password = requesterRegisterDto.Password };
        var requesterLogin = await LoginUserSuccessfullyAsync(requesterLoginDto); // Pass DTO
        var requesterToken = requesterLogin.AccessToken;
        var updateRolesDto = new UpdateUserRolesDto { Roles = new List<string> { "Admin" } };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", requesterToken);
        var response = await _client.PutAsJsonAsync($"{ApiConstants.BasePaths.Users}/{targetUserId}/roles", updateRolesDto);
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
        var response = await _client.PutAsJsonAsync($"{ApiConstants.BasePaths.Users}/{nonExistentUserId}/roles", updateRolesDto);
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUserRoles_WithoutToken_ReturnsUnauthorized()
    {
        var targetUserId = Guid.NewGuid();
        var updateRolesDto = new UpdateUserRolesDto { Roles = new List<string> { "Admin" } };
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PutAsJsonAsync($"{ApiConstants.BasePaths.Users}/{targetUserId}/roles", updateRolesDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
