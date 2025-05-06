using FluentAssertions;
using JaCore.Api.DTOs.Device;
using JaCore.Api.Helpers;
using JaCore.Api.IntegrationTests.Controllers.Base;
using JaCore.Api.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using JaCore.Common.Device;
using Microsoft.Extensions.Logging;
using JaCore.Api.DTOs.Auth;
using Xunit;

namespace JaCore.Api.IntegrationTests.Controllers.Device;

public class DeviceIntegrationTests : AuthTestsBase
{
    public DeviceIntegrationTests(CustomWebApplicationFactory factory) : base(factory) { }

    private const string DevicesBaseUrl = ApiConstants.BasePaths.Devices;

    private async Task<(Guid UserId, string AccessToken)> GetUserIdAndTokenAsync(string email = "admin@jacore.app", string password = "AdminPassword123!")
    {
        _logger.LogInformation("Attempting login for test setup with email: {Email}", email);
        var loginDto = new LoginDto(email, password);
        var (loginResult, loginResponse) = await LoginUserAsync(loginDto);

        if (!loginResponse.IsSuccessStatusCode || loginResult == null)
        {
            var errorContent = await loginResponse.Content.ReadAsStringAsync();
            _logger.LogError("Login failed during test setup for {Email}. Status: {StatusCode}. Response: {ErrorContent}",
                             email, loginResponse.StatusCode, errorContent);
            loginResponse.EnsureSuccessStatusCode();
            throw new InvalidOperationException("Login failed during test setup.");
        }

        loginResult.Should().NotBeNull();
        loginResult.Succeeded.Should().BeTrue();
        loginResult.AccessToken.Should().NotBeNullOrWhiteSpace();
        loginResult.UserId.Should().NotBeEmpty();
        loginResult.UserId.Should().NotBe(Guid.Empty);
        loginResult.UserId.Should().NotBeNull();
        loginResult.Email.Should().Be(email);
     
        if (loginResult.UserId == Guid.Empty || loginResult.UserId == null)
        {
            _logger.LogError("Login returned an empty UserId for {Email}", email);
            throw new InvalidOperationException("Login returned an empty UserId.");
        }
     
        _logger.LogInformation("Login successful for {Email}. User ID: {UserId}", email, loginResult.UserId);
        return (loginResult.UserId, loginResult.AccessToken);
    }


    [Fact]
    public async Task CreateDevice_WithValidData_ReturnsCreated()
    {
        // Arrange
        var (_, accessToken) = await GetUserIdAndTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createDto = new DeviceCreateDto(
            Name: "Test Device",
            SerialNumber: "SN12345",
            PurchaseDate: DateTimeOffset.UtcNow.AddMonths(-6),
            CategoryId: null,
            SupplierId: null
        );

        // Act
        var response = await _client.PostAsJsonAsync(DevicesBaseUrl, createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdDevice = await response.Content.ReadFromJsonAsync<DeviceDto>();
        createdDevice.Should().NotBeNull();
        createdDevice!.Name.Should().Be(createDto.Name);
        createdDevice.SerialNumber.Should().Be(createDto.SerialNumber);
    }

    [Fact]
    public async Task GetDevice_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null; // Ensure no auth header

        // Act
        var response = await _client.GetAsync($"{DevicesBaseUrl}/{Guid.NewGuid()}"); // Use a dummy Guid

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SomeTestCheckingStatusCodeRange()
    {
        // Arrange
        var (_, accessToken) = await GetUserIdAndTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var someId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"{DevicesBaseUrl}/{someId}"); // Example request

        // Assert
        response.StatusCode.Should().Match(s => s >= HttpStatusCode.OK && s < HttpStatusCode.MultipleChoices, "because the request should be successful (2xx)");
    }
}