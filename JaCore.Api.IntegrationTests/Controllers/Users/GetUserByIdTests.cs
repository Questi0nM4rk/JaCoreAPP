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
public class GetUserByIdTests : AuthTestsBase
{
    public GetUserByIdTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetUserById_AdminGettingAnyUser_ReturnsOkAndUserData()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync(); // Get admin token first
        var registerDto = new RegisterDto
        {
            Email = $"target-get-{Guid.NewGuid()}@example.com",
            FirstName = "Target",
            LastName = "UserGet",
            Password = "Password123!"
        };
        var targetUser = await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO
        var targetUserId = targetUser.UserId;
        targetUserId.Should().NotBeNullOrEmpty();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.GetAsync($"{ApiConstants.BasePaths.Users}/{targetUserId}");
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
        var registerDto = new RegisterDto
        {
            Email = $"self-get-{Guid.NewGuid()}@example.com",
            FirstName = "Self",
            LastName = "Get",
            Password = "Password123!"
        };
        var registerResult = await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto); // Pass token and DTO

        var loginDto = new LoginDto { Email = registerDto.Email, Password = registerDto.Password };
        var loginResult = await LoginUserSuccessfullyAsync(loginDto); // Pass DTO
        var userId = registerResult.UserId;
        var userToken = loginResult.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var response = await _client.GetAsync($"{ApiConstants.BasePaths.Users}/{userId}");
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
        var targetRegisterDto = new RegisterDto
        {
            Email = $"target-other-get-{Guid.NewGuid()}@example.com",
            FirstName = "TargetOther",
            LastName = "Get",
            Password = "Password123!"
        };
        var targetUser = await RegisterUserSuccessfullyAsync(adminAccessToken, targetRegisterDto); // Pass token and DTO
        var targetUserId = targetUser.UserId;

        var requesterRegisterDto = new RegisterDto
        {
            Email = $"requester-other-{Guid.NewGuid()}@example.com",
            FirstName = "Requester",
            LastName = "Other",
            Password = "Password123!"
        };
        await RegisterUserSuccessfullyAsync(adminAccessToken, requesterRegisterDto); // Pass token and DTO

        var requesterLoginDto = new LoginDto { Email = requesterRegisterDto.Email, Password = requesterRegisterDto.Password };
        var requesterLogin = await LoginUserSuccessfullyAsync(requesterLoginDto); // Pass DTO
        var requesterToken = requesterLogin.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", requesterToken);
        var response = await _client.GetAsync($"{ApiConstants.BasePaths.Users}/{targetUserId}");
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserById_UserNotFound_ReturnsNotFound()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync();
        var nonExistentUserId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.GetAsync($"{ApiConstants.BasePaths.Users}/{nonExistentUserId}");
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserById_WithoutToken_ReturnsUnauthorized()
    {
        var targetUserId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync($"{ApiConstants.BasePaths.Users}/{targetUserId}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
