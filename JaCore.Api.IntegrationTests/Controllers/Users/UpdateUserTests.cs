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
public class UpdateUserTests : AuthTestsBase
{
    private const string UsersBaseUrl = $"{ApiConstants.ApiRoutePrefix}/users";

    public UpdateUserTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task UpdateUser_AdminUpdatingAnyUser_ReturnsNoContent()
    {
        var targetUser = await RegisterUserSuccessfullyAsync(firstName: "TargetUpdate", lastName: "Initial");
        var targetUserId = targetUser.UserId;
        var adminAccessToken = await GetAdminAccessTokenAsync();
        var updateDto = new UpdateUserDto { FirstName = "TargetUpdated", LastName = "ByAdmin" };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.PutAsJsonAsync($"{UsersBaseUrl}/{targetUserId}", updateDto);
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateUser_UserUpdatingSelf_ReturnsNoContent()
    {
        var userEmail = $"self-update-{Guid.NewGuid()}@example.com";
        var userPassword = "Password123!";
        var registerResult = await RegisterUserSuccessfullyAsync(userEmail, userPassword, firstName: "SelfUpdate", lastName: "Initial");
        var loginResult = await LoginUserSuccessfullyAsync(userEmail, userPassword);
        var userId = registerResult.UserId;
        var userToken = loginResult.AccessToken;
        var updateDto = new UpdateUserDto { FirstName = "SelfUpdated", LastName = "ByUser" };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var response = await _client.PutAsJsonAsync($"{UsersBaseUrl}/{userId}", updateDto);
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateUser_UserUpdatingOtherUser_ReturnsForbidden()
    {
        var targetUser = await RegisterUserSuccessfullyAsync(firstName: "TargetOtherUpdate");
        var targetUserId = targetUser.UserId;
        var requesterEmail = $"requester-other-update-{Guid.NewGuid()}@example.com";
        var requesterPassword = "Password123!";
        await RegisterUserSuccessfullyAsync(requesterEmail, requesterPassword);
        var requesterLogin = await LoginUserSuccessfullyAsync(requesterEmail, requesterPassword);
        var requesterToken = requesterLogin.AccessToken;
        var updateDto = new UpdateUserDto { FirstName = "AttemptedUpdate", LastName = "Forbidden" };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", requesterToken);
        var response = await _client.PutAsJsonAsync($"{UsersBaseUrl}/{targetUserId}", updateDto);
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateUser_UserNotFound_ReturnsNotFound()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync();
        var nonExistentUserId = Guid.NewGuid();
        var updateDto = new UpdateUserDto { FirstName = "NotFoundUpdate", LastName = "NotFound" };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.PutAsJsonAsync($"{UsersBaseUrl}/{nonExistentUserId}", updateDto);
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUser_WithoutToken_ReturnsUnauthorized()
    {
        var targetUserId = Guid.NewGuid();
        var updateDto = new UpdateUserDto { FirstName = "UnauthorizedUpdate", LastName = "Unauthorized" };
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PutAsJsonAsync($"{UsersBaseUrl}/{targetUserId}", updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUser_WithInvalidData_ReturnsBadRequest()
    {
        var userEmail = $"self-update-invalid-{Guid.NewGuid()}@example.com";
        var userPassword = "Password123!";
        var registerResult = await RegisterUserSuccessfullyAsync(userEmail, userPassword);
        var loginResult = await LoginUserSuccessfullyAsync(userEmail, userPassword);
        var userId = registerResult.UserId;
        var userToken = loginResult.AccessToken;
        var updateDto = new UpdateUserDto { FirstName = "", LastName = "ValidLastName" };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var response = await _client.PutAsJsonAsync($"{UsersBaseUrl}/{userId}", updateDto);
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private class UpdateUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
