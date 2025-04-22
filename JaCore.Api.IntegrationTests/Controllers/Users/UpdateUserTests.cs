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
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterUserDto(
            Email: $"target-update-{Guid.NewGuid()}@example.com",
            FirstName: "TargetUpdate",
            LastName: "Initial",
            Password: "Password123!"
        );
        var targetUser = await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO
        var targetUserId = targetUser.UserId;
        var updateDto = new UpdateUserDto { FirstName = "TargetUpdated", LastName = "ByAdmin" };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.PutAsJsonAsync($"{UsersBaseUrl}/{targetUserId}", updateDto);
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateUser_UserUpdatingSelf_ReturnsNoContent()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterUserDto(
            Email: $"self-update-{Guid.NewGuid()}@example.com",
            FirstName: "SelfUpdate",
            LastName: "Initial",
            Password: "Password123!"
        );
        var registerResult = await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO

        var loginDto = new LoginUserDto(registerDto.Email, registerDto.Password);
        var loginResult = await LoginUserSuccessfullyAsync(loginDto); // Pass DTO
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
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var targetRegisterDto = new RegisterUserDto(
            Email: $"target-other-update-{Guid.NewGuid()}@example.com",
            FirstName: "TargetOtherUpdate",
            LastName: "Initial",
            Password: "Password123!"
        );
        var targetUser = await RegisterUserSuccessfullyAsync(adminAccessToken, targetRegisterDto); // Pass token and DTO
        var targetUserId = targetUser.UserId;

        var requesterRegisterDto = new RegisterUserDto(
            Email: $"requester-other-update-{Guid.NewGuid()}@example.com",
            FirstName: "Requester",
            LastName: "OtherUpdate",
            Password: "Password123!"
        );
        await RegisterUserSuccessfullyAsync(adminAccessToken, requesterRegisterDto); // Pass token and DTO

        var requesterLoginDto = new LoginUserDto(requesterRegisterDto.Email, requesterRegisterDto.Password);
        var requesterLogin = await LoginUserSuccessfullyAsync(requesterLoginDto); // Pass DTO
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
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterUserDto(
            Email: $"self-update-invalid-{Guid.NewGuid()}@example.com",
            FirstName: "SelfUpdateInvalid",
            LastName: "Initial",
            Password: "Password123!"
        );
        var registerResult = await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO

        var loginDto = new LoginUserDto(registerDto.Email, registerDto.Password);
        var loginResult = await LoginUserSuccessfullyAsync(loginDto); // Pass DTO
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
