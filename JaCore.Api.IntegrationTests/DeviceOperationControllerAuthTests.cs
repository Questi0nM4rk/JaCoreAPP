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
public class DeviceOperationControllerAuthTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _anonClient;

    public DeviceOperationControllerAuthTests(ApiWebApplicationFactory factory)
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
    
    private async Task<int> CreateTestDeviceAsync(HttpClient? client = null)
    {
        client ??= await GetAuthenticatedClientAsync("Admin");
        var createDto = new CreateDeviceDto { Name = $"DeviceForOpCard {Guid.NewGuid()}", OrderIndex = 1 };
        var response = await client.PostAsJsonAsync("/api/Device", createDto);
        response.EnsureSuccessStatusCode();
        var device = await response.Content.ReadFromJsonAsync<DeviceDto>();
        return device?.Id ?? throw new Exception("Failed to create test device for operation tests");
    }

    private async Task<int> CreateTestDeviceCardAsync(HttpClient? adminClient = null)
    {
        adminClient ??= await GetAuthenticatedClientAsync("Admin");
        var deviceId = await CreateTestDeviceAsync(adminClient);
        var createDto = new CreateDeviceCardDto { SerialNumber = $"TESTCARD-OP-{Guid.NewGuid()}" };
        var response = await adminClient.PostAsJsonAsync($"/api/DeviceCard/ForDevice/{deviceId}", createDto);
        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            throw new Exception("Conflict creating device card for operation tests.");
        response.EnsureSuccessStatusCode();
        var card = await response.Content.ReadFromJsonAsync<DeviceCardDto>();
        return card?.Id ?? throw new Exception("Failed to create test device card for operation tests");
    }

    private async Task<int> CreateTestOperationAsync(int deviceCardId, HttpClient? client = null)
    {
        client ??= await GetAuthenticatedClientAsync("Admin");
        var createDto = new CreateDeviceOperationDto 
        { 
            DeviceCardId = deviceCardId, 
            Name = $"Test Operation {Guid.NewGuid()}",
            OrderIndex = 1
        };
        var response = await client.PostAsJsonAsync("/api/DeviceOperation", createDto);
        response.EnsureSuccessStatusCode();
        var op = await response.Content.ReadFromJsonAsync<DeviceOperationDto>();
        return op?.Id ?? throw new Exception("Failed to create test device operation");
    }

    // --- Endpoint Tests --- 

    // GET /api/DeviceOperation?deviceCardId={id}
    [Theory]
    [InlineData("Admin", HttpStatusCode.OK)]
    [InlineData("Management", HttpStatusCode.OK)]
    [InlineData("User", HttpStatusCode.OK)]
    [InlineData("Debug", HttpStatusCode.OK)]
    public async Task GetByDeviceCardId_WithRequiredRole_ReturnsOk(string role, HttpStatusCode expectedStatus)
    {
        var cardId = await CreateTestDeviceCardAsync();
        var client = await GetAuthenticatedClientAsync(role);
        var response = await client.GetAsync($"/api/DeviceOperation?deviceCardId={cardId}");
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task GetByDeviceCardId_Anonymous_ReturnsUnauthorized() => await TestAnonymous($"/api/DeviceOperation?deviceCardId=1", HttpMethod.Get);
    [Fact] public async Task GetByDeviceCardId_NoCardId_ReturnsBadRequest() 
    { 
        var client = await GetAuthenticatedClientAsync("User");
        var response = await client.GetAsync("/api/DeviceOperation");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // GET /api/DeviceOperation/{id}
    [Theory]
    [InlineData("Admin", HttpStatusCode.OK)]
    [InlineData("Management", HttpStatusCode.OK)]
    [InlineData("User", HttpStatusCode.OK)]
    [InlineData("Debug", HttpStatusCode.OK)]
    public async Task GetById_WithRequiredRole_ReturnsOk(string role, HttpStatusCode expectedStatus)
    {
        var cardId = await CreateTestDeviceCardAsync();
        var opId = await CreateTestOperationAsync(cardId);
        var client = await GetAuthenticatedClientAsync(role);
        var response = await client.GetAsync($"/api/DeviceOperation/{opId}");
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task GetById_Anonymous_ReturnsUnauthorized() => await TestAnonymous($"/api/DeviceOperation/1", HttpMethod.Get);

    // POST /api/DeviceOperation
    [Theory]
    [InlineData("Admin", HttpStatusCode.Created)]
    [InlineData("Management", HttpStatusCode.Created)]
    [InlineData("Debug", HttpStatusCode.Created)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    public async Task Create_WithRole_ReturnsExpectedStatus(string role, HttpStatusCode expectedStatus)
    {
        var cardId = await CreateTestDeviceCardAsync();
        var client = await GetAuthenticatedClientAsync(role);
        var createDto = new CreateDeviceOperationDto { DeviceCardId = cardId, Name = $"Create Op {role}", OrderIndex = 1 };
        var response = await client.PostAsJsonAsync("/api/DeviceOperation", createDto);
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task Create_Anonymous_ReturnsUnauthorized() => await TestAnonymous("/api/DeviceOperation", HttpMethod.Post, new CreateDeviceOperationDto { DeviceCardId = 1, Name = "Anon Op"});
    [Fact] public async Task Create_NoCardId_ReturnsBadRequest()
    { 
        var client = await GetAuthenticatedClientAsync("Admin");
        var createDto = new { Name = "Op No Card ID", OrderIndex = 1 };
        var response = await client.PostAsJsonAsync("/api/DeviceOperation", createDto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // PUT /api/DeviceOperation/{id}
    [Theory]
    [InlineData("Admin", HttpStatusCode.NoContent)]
    [InlineData("Management", HttpStatusCode.NoContent)]
    [InlineData("Debug", HttpStatusCode.NoContent)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    public async Task Update_WithRole_ReturnsExpectedStatus(string role, HttpStatusCode expectedStatus)
    {
        var cardId = await CreateTestDeviceCardAsync();
        var opId = await CreateTestOperationAsync(cardId);
        var client = await GetAuthenticatedClientAsync(role);
        var updateDto = new UpdateDeviceOperationDto { Name = $"Update Op {role}", OrderIndex = 1 };
        var response = await client.PutAsJsonAsync($"/api/DeviceOperation/{opId}", updateDto);
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task Update_Anonymous_ReturnsUnauthorized() => await TestAnonymous($"/api/DeviceOperation/1", HttpMethod.Put, new UpdateDeviceOperationDto { Name = "Anon Update" });

    // DELETE /api/DeviceOperation/{id}
    [Theory]
    [InlineData("Admin", HttpStatusCode.NoContent)]
    [InlineData("Debug", HttpStatusCode.NoContent)]
    [InlineData("Management", HttpStatusCode.Forbidden)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    public async Task Delete_WithRole_ReturnsExpectedStatus(string role, HttpStatusCode expectedStatus)
    {
        var cardId = await CreateTestDeviceCardAsync();
        var opId = await CreateTestOperationAsync(cardId); 
        var client = await GetAuthenticatedClientAsync(role);
        var response = await client.DeleteAsync($"/api/DeviceOperation/{opId}");
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task Delete_Anonymous_ReturnsUnauthorized() => await TestAnonymous($"/api/DeviceOperation/1", HttpMethod.Delete);

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