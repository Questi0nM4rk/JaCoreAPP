using FluentAssertions;
using JaCore.Api.Helpers;
using JaCore.Api.IntegrationTests.Controllers.Auth;
using JaCore.Api.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Headers;
using Xunit;
using JaCore.Api.IntegrationTests.DTOs.Auth;
using JaCore.Api.IntegrationTests.Controllers.Base;
using System.Net.Http.Json;
using JaCore.Api.DTOs.Users;

namespace JaCore.Api.IntegrationTests.Controllers.Users;

[Collection("Database Collection")]
public class DeleteUserTests : AuthTestsBase
{
    public DeleteUserTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task SoftDeleteUser_AdminDeletingUser_ReturnsNoContent()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterDto
        {
            Email = $"target-delete-{Guid.NewGuid()}@example.com",
            FirstName = "Target",
            LastName = "Delete",
            Password = "Password123!"
        };
        var targetUser = await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO
        var targetUserId = targetUser.UserId;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.DeleteAsync($"{ApiConstants.BasePaths.Users}/{targetUserId}");
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify user is actually deleted
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var getResponse = await _client.GetAsync($"{ApiConstants.BasePaths.Users}/{targetUserId}");
        _client.DefaultRequestHeaders.Authorization = null;
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var userAfterDelete = await getResponse.Content.ReadFromJsonAsync<UserDto>();
        userAfterDelete.Should().NotBeNull();
        userAfterDelete.IsActive.Should().BeFalse(); // Assuming IsActive is a property that indicates if the user is deleted
    }

    [Fact]
    public async Task DeleteUser_UserDeletingOtherUser_ReturnsForbidden()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var targetRegisterDto = new RegisterDto
        {
            Email = $"target-forbidden-delete-{Guid.NewGuid()}@example.com",
            FirstName = "Target",
            LastName = "Forbidden",
            Password = "Password123!"
        };
        var targetUser = await RegisterUserSuccessfullyAsync(adminAccessToken, targetRegisterDto); // Pass token and DTO
        var targetUserId = targetUser.UserId;

        var requesterRegisterDto = new RegisterDto
        {
            Email = $"requester-delete-{Guid.NewGuid()}@example.com",
            FirstName = "Requester",
            LastName = "Delete",
            Password = "Password123!"
        };
        await RegisterUserSuccessfullyAsync(adminAccessToken, requesterRegisterDto); // Pass token and DTO

        var requesterLoginDto = new LoginDto { Email = requesterRegisterDto.Email, Password = requesterRegisterDto.Password };
        var requesterLogin = await LoginUserSuccessfullyAsync(requesterLoginDto); // Pass DTO
        var requesterToken = requesterLogin.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", requesterToken);
        var response = await _client.DeleteAsync($"{ApiConstants.BasePaths.Users}/{targetUserId}");
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteUser_UserNotFound_ReturnsNotFound()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync();
        var nonExistentUserId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.DeleteAsync($"{ApiConstants.BasePaths.Users}/{nonExistentUserId}");
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_WithoutToken_ReturnsUnauthorized()
    {
        var targetUserId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.DeleteAsync($"{ApiConstants.BasePaths.Users}/{targetUserId}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
