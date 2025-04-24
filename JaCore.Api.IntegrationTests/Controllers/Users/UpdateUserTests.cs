using FluentAssertions;
using JaCore.Api.Helpers;
using JaCore.Api.IntegrationTests.Controllers.Auth;
using JaCore.Api.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;
using JaCore.Api.IntegrationTests.DTOs.Auth;
using JaCore.Api.IntegrationTests.DTOs.Users; // Ensure this is present for UserDto
using JaCore.Api.IntegrationTests.Controllers.Base;

namespace JaCore.Api.IntegrationTests.Controllers.Users;

[Collection("Database Collection")]
public class UpdateUserTests : AuthTestsBase
{
    public UpdateUserTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task UpdateUser_AdminUpdatingAnyUser_ReturnsNoContent()
    {
        // Arrange
        var adminAccessToken = await GetAdminAccessTokenAsync();
        var registerDto = new RegisterDto
        {
            Email = $"target-update-{Guid.NewGuid()}@example.com",
            FirstName = "TargetUpdate",
            LastName = "Initial",
            Password = "Password123!"
        };
        
        // Act - Register target user
        var targetUser = await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto);
        
        // Assert initial registration
        targetUser.UserId.Should().NotBeNullOrWhiteSpace();
        targetUser.Email.Should().Be(registerDto.Email);
        targetUser.FirstName.Should().Be(registerDto.FirstName);
        targetUser.LastName.Should().Be(registerDto.LastName);

        var targetUserId = targetUser.UserId;
        var updateDto = new UpdateUserDto
        {
            FirstName = "TargetUpdated", 
            LastName = "ByAdmin", 
            Email = "UpdateMail2@jacore.com"
        };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);

        // Act - Update user
        var response = await _client.PutAsJsonAsync($"{ApiConstants.BasePaths.Users}/{targetUserId}", updateDto);
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update by fetching the user again
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var getUserAfterUpdateResponse = await _client.GetAsync($"{ApiConstants.BasePaths.Users}/{targetUserId}");
        _client.DefaultRequestHeaders.Authorization = null;
        getUserAfterUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var userAfterUpdate = await getUserAfterUpdateResponse.Content.ReadFromJsonAsync<UserDto>(); // Use Integration Test UserDto
        userAfterUpdate.Should().NotBeNull();
        userAfterUpdate!.FirstName.Should().Be(updateDto.FirstName);
        userAfterUpdate.LastName.Should().Be(updateDto.LastName);
        userAfterUpdate.Email.Should().Be(updateDto.Email);
    }

    [Fact]
    public async Task UpdateUser_UserUpdatingSelf_ReturnsNoContent()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync();
        var registerDto = new RegisterDto
        {
            Email = $"self-update-{Guid.NewGuid()}@example.com",
            FirstName = "SelfUpdate",
            LastName = "Initial",
            Password = "Password123!"
        };
        var registerResult = await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto);

        var loginDto = new LoginDto { Email = registerDto.Email, Password = registerDto.Password };
        var loginResult = await LoginUserSuccessfullyAsync(loginDto);
        var userId = registerResult.UserId;
        var userToken = loginResult.AccessToken;
        var updateDto = new UpdateUserDto
        {
            FirstName = "SelfUpdated", 
            LastName = "ByUser", 
            Email = "UpdateMail@jacore.com"
        };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var response = await _client.PutAsJsonAsync($"{ApiConstants.BasePaths.Users}/{userId}", updateDto);
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update by fetching the user again (using the same user's token)
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var getUserAfterUpdateResponse = await _client.GetAsync($"{ApiConstants.BasePaths.Users}/{userId}");
        _client.DefaultRequestHeaders.Authorization = null;
        getUserAfterUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var userAfterUpdate = await getUserAfterUpdateResponse.Content.ReadFromJsonAsync<UserDto>(); // Use Integration Test UserDto
        userAfterUpdate.Should().NotBeNull();
        userAfterUpdate!.FirstName.Should().Be(updateDto.FirstName);
        userAfterUpdate.LastName.Should().Be(updateDto.LastName);
        userAfterUpdate.Email.Should().Be(updateDto.Email);
    }

    [Fact]
    public async Task UpdateUser_UserUpdatingOtherUser_ReturnsForbidden()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync();
        var targetRegisterDto = new RegisterDto
        {
            Email = $"target-other-update-{Guid.NewGuid()}@example.com",
            FirstName = "TargetOtherUpdate",
            LastName = "Initial",
            Password = "Password123!"
        };
        var targetUser = await RegisterUserSuccessfullyAsync(adminAccessToken, targetRegisterDto);
        var targetUserId = targetUser.UserId;

        var requesterRegisterDto = new RegisterDto
        {
            Email = $"requester-other-update-{Guid.NewGuid()}@example.com",
            FirstName = "Requester",
            LastName = "OtherUpdate",
            Password = "Password123!"
        };
        await RegisterUserSuccessfullyAsync(adminAccessToken, requesterRegisterDto);

        var requesterLoginDto = new LoginDto { Email = requesterRegisterDto.Email, Password = requesterRegisterDto.Password };
        var requesterLogin = await LoginUserSuccessfullyAsync(requesterLoginDto);
        var requesterToken = requesterLogin.AccessToken;
        var updateDto = new UpdateUserDto
        {
            FirstName = "AttemptedUpdate", 
            LastName = "Forbidden", 
            Email = "UpdateMail@jacore.com"
        };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", requesterToken);
        var response = await _client.PutAsJsonAsync($"{ApiConstants.BasePaths.Users}/{targetUserId}", updateDto);
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateUser_UserNotFound_ReturnsNotFound()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync();
        var nonExistentUserId = Guid.NewGuid();
        var updateDto = new UpdateUserDto
        {
            FirstName = "NotFoundUpdate", 
            LastName = "NotFound", 
            Email = "UpdateMail@jacore.com"
        };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.PutAsJsonAsync($"{ApiConstants.BasePaths.Users}/{nonExistentUserId}", updateDto);
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUser_WithoutToken_ReturnsUnauthorized()
    {
        var targetUserId = Guid.NewGuid();
        var updateDto = new UpdateUserDto
        {
            FirstName = "UnauthorizedUpdate", 
            LastName = "Unauthorized", 
            Email = "UpdateMail@jacore.com"
        };
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PutAsJsonAsync($"{ApiConstants.BasePaths.Users}/{targetUserId}", updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUser_WithInvalidData_ReturnsBadRequest()
    {
        var adminAccessToken = await GetAdminAccessTokenAsync();
        var registerDto = new RegisterDto
        {
            Email = $"self-update-invalid-{Guid.NewGuid()}@example.com",
            FirstName = "SelfUpdateInvalid",
            LastName = "Initial",
            Password = "Password123!"
        };
        var registerResult = await RegisterUserSuccessfullyAsync(adminAccessToken, registerDto);

        var loginDto = new LoginDto { Email = registerDto.Email, Password = registerDto.Password };
        var loginResult = await LoginUserSuccessfullyAsync(loginDto);
        var userId = registerResult.UserId;
        var userToken = loginResult.AccessToken;
        var updateDto = new UpdateUserDto
        {
            FirstName = "", 
            LastName = "ValidLastName", 
            Email = "UpdateMail@jacore.com"
        };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var response = await _client.PutAsJsonAsync($"{ApiConstants.BasePaths.Users}/{userId}", updateDto);
        _client.DefaultRequestHeaders.Authorization = null;
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
