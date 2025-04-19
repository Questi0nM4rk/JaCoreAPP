using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Dtos.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using JaCore.Api.Models.User;

namespace JaCore.Api.IntegrationTests;

[Collection("Sequential")] 
public class DeviceCardControllerAuthTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _anonClient;

    public DeviceCardControllerAuthTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _anonClient = _factory.CreateClient();
    }

    // --- Helper Methods ---
    private async Task<HttpClient> GetAuthenticatedClientAsync(string role)
    {
        string email;
        switch (role.ToLower())
        {
            case "admin": email = ApiWebApplicationFactory.AdminEmail; break;
            case "management": email = ApiWebApplicationFactory.ManagementEmail; await EnsureUserExists(email, role); break;
            case "user": email = ApiWebApplicationFactory.UserEmail; await EnsureUserExists(email, role); break;
            case "debug": email = ApiWebApplicationFactory.DebugEmail; await EnsureUserExists(email, role, "Admin"); break;
            default: throw new ArgumentException($"Unknown role: {role}", nameof(role));
        }

        var loginDto = new UserLoginDto { Email = email, Password = ApiWebApplicationFactory.DefaultPassword };
        var loginResponse = await _anonClient.PostAsJsonAsync("/api/Auth/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        if (authResponse == null || !authResponse.IsSuccess || string.IsNullOrEmpty(authResponse.Token))
            throw new Exception($"Test setup failed: Login as {role} ({email})");

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);
        return client;
    }

    private async Task EnsureUserExists(string email, string role, string? identityRole = null)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FirstName = role, LastName = "Test", IsActive = true };
            var result = await userManager.CreateAsync(user, ApiWebApplicationFactory.DefaultPassword);
            if (!result.Succeeded) throw new Exception($"Failed to seed {role} user: {string.Join(",", result.Errors.Select(e => e.Description))}");
            result = await userManager.AddToRoleAsync(user, identityRole ?? role);
            if (!result.Succeeded) throw new Exception($"Failed to add {role} user to role: {string.Join(",", result.Errors.Select(e => e.Description))}");
        }
    }

    // Helper to create a Device (needed to associate a card)
    private async Task<int> CreateTestDeviceAsync(HttpClient? client = null)
    {
        client ??= await GetAuthenticatedClientAsync("Admin");
        var createDto = new CreateDeviceDto { Name = $"DeviceForCard {Guid.NewGuid()}", OrderIndex = 1 };
        var response = await client.PostAsJsonAsync("/api/Device", createDto);
        response.EnsureSuccessStatusCode();
        var device = await response.Content.ReadFromJsonAsync<DeviceDto>();
        return device?.Id ?? throw new Exception("Failed to create test device for card tests");
    }
    
    // Helper to create a Device Card linked to a device
    private async Task<(int DeviceId, int CardId)> CreateTestDeviceCardAsync(HttpClient? client = null)
    {
        client ??= await GetAuthenticatedClientAsync("Admin");
        var deviceId = await CreateTestDeviceAsync(client);
        var createDto = new CreateDeviceCardDto { SerialNumber = $"CARD-{Guid.NewGuid()}" };
        var response = await client.PostAsJsonAsync($"/api/DeviceCard/ForDevice/{deviceId}", createDto);
        response.EnsureSuccessStatusCode();
        var card = await response.Content.ReadFromJsonAsync<DeviceCardDto>();
        return (deviceId, card?.Id ?? throw new Exception("Failed to create test device card"));
    }

    // --- Endpoint Tests --- 

    // GET /api/DeviceCard/ByDevice/{deviceId}
    [Theory]
    [InlineData("Admin", HttpStatusCode.OK)]
    [InlineData("Management", HttpStatusCode.OK)]
    [InlineData("User", HttpStatusCode.OK)]
    [InlineData("Debug", HttpStatusCode.OK)]
    public async Task GetByDeviceId_WithRequiredRole_ReturnsOk(string role, HttpStatusCode expectedStatus)
    {
        var (deviceId, _) = await CreateTestDeviceCardAsync();
        var client = await GetAuthenticatedClientAsync(role);
        var response = await client.GetAsync($"/api/DeviceCard/ByDevice/{deviceId}");
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task GetByDeviceId_Anonymous_ReturnsUnauthorized() => await TestAnonymous($"/api/DeviceCard/ByDevice/1", HttpMethod.Get);

    // GET /api/DeviceCard/{id}
    [Theory]
    [InlineData("Admin", HttpStatusCode.OK)]
    [InlineData("Management", HttpStatusCode.OK)]
    [InlineData("User", HttpStatusCode.OK)]
    [InlineData("Debug", HttpStatusCode.OK)]
    public async Task GetById_WithRequiredRole_ReturnsOk(string role, HttpStatusCode expectedStatus)
    {
        var (_, cardId) = await CreateTestDeviceCardAsync();
        var client = await GetAuthenticatedClientAsync(role);
        var response = await client.GetAsync($"/api/DeviceCard/{cardId}");
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task GetById_Anonymous_ReturnsUnauthorized() => await TestAnonymous($"/api/DeviceCard/1", HttpMethod.Get);

    // POST /api/DeviceCard/ForDevice/{deviceId}
    [Theory]
    [InlineData("Admin", HttpStatusCode.Created)]
    [InlineData("Management", HttpStatusCode.Created)]
    [InlineData("Debug", HttpStatusCode.Created)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    public async Task Create_WithRole_ReturnsExpectedStatus(string role, HttpStatusCode expectedStatus)
    {
        var client = await GetAuthenticatedClientAsync(role);
        // Create device *without* a card first
        var deviceId = await CreateTestDeviceAsync(await GetAuthenticatedClientAsync("Admin")); 
        var createDto = new CreateDeviceCardDto { SerialNumber = $"CreateCard-{role}-{Guid.NewGuid()}" };
        var response = await client.PostAsJsonAsync($"/api/DeviceCard/ForDevice/{deviceId}", createDto);
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task Create_Anonymous_ReturnsUnauthorized() => await TestAnonymous($"/api/DeviceCard/ForDevice/1", HttpMethod.Post, new CreateDeviceCardDto());

    // PUT /api/DeviceCard/{id}
    [Theory]
    [InlineData("Admin", HttpStatusCode.NoContent)]
    [InlineData("Management", HttpStatusCode.NoContent)]
    [InlineData("Debug", HttpStatusCode.NoContent)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    public async Task Update_WithRole_ReturnsExpectedStatus(string role, HttpStatusCode expectedStatus)
    {
        var (_, cardId) = await CreateTestDeviceCardAsync();
        var client = await GetAuthenticatedClientAsync(role);
        var updateDto = new UpdateDeviceCardDto { SerialNumber = $"UpdateCard-{role}-{Guid.NewGuid()}" };
        var response = await client.PutAsJsonAsync($"/api/DeviceCard/{cardId}", updateDto);
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task Update_Anonymous_ReturnsUnauthorized() => await TestAnonymous($"/api/DeviceCard/1", HttpMethod.Put, new UpdateDeviceCardDto());

    // DELETE /api/DeviceCard/{id}
    [Theory]
    [InlineData("Admin", HttpStatusCode.NoContent)]
    [InlineData("Debug", HttpStatusCode.NoContent)]
    [InlineData("Management", HttpStatusCode.Forbidden)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    public async Task Delete_WithRole_ReturnsExpectedStatus(string role, HttpStatusCode expectedStatus)
    {
        var (_, cardId) = await CreateTestDeviceCardAsync(); 
        var client = await GetAuthenticatedClientAsync(role);
        var response = await client.DeleteAsync($"/api/DeviceCard/{cardId}");
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task Delete_Anonymous_ReturnsUnauthorized() => await TestAnonymous($"/api/DeviceCard/1", HttpMethod.Delete);

    // --- Shared Test Helpers ---
    private async Task TestAnonymous(string url, HttpMethod method, object? content = null)
    {
        HttpRequestMessage request = new HttpRequestMessage(method, url);
        if (content != null && (method == HttpMethod.Post || method == HttpMethod.Put))
        {
            request.Content = JsonContent.Create(content);
        }
        var response = await _anonClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
} 