using System.Collections.Generic;
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

[Collection("Sequential")] // Use if tests interfere due to shared DB state, otherwise remove
public class DeviceControllerAuthTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _anonClient; // Unauthenticated client

    public DeviceControllerAuthTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _anonClient = _factory.CreateClient();
    }

    // --- Helper Methods (Specific to this test class if needed, or use shared helpers) ---
    private async Task<HttpClient> GetAuthenticatedClientAsync(string role)
    {
        string email;
        switch (role.ToLower())
        {
            case "admin":
                email = ApiWebApplicationFactory.AdminEmail;
                break;
            // Add cases for seeding/retrieving Manager, User, Debug if needed
            // For now, we can register them on the fly if they don't exist
            case "management":
                email = ApiWebApplicationFactory.ManagementEmail;
                await EnsureUserExists(email, role); 
                break;
            case "user":
                 email = ApiWebApplicationFactory.UserEmail;
                 await EnsureUserExists(email, role);
                 break;
            case "debug":
                 email = ApiWebApplicationFactory.DebugEmail;
                 await EnsureUserExists(email, role, "Admin"); // Debug often maps to Admin role
                 break;
            default:
                throw new ArgumentException($"Unknown role for test client: {role}", nameof(role));
        }

        // Login
        var loginDto = new UserLoginDto { Email = email, Password = ApiWebApplicationFactory.DefaultPassword };
        var loginResponse = await _anonClient.PostAsJsonAsync("/api/Auth/login", loginDto);
        loginResponse.EnsureSuccessStatusCode(); // Throw if login fails
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        if (authResponse == null || !authResponse.IsSuccess || string.IsNullOrEmpty(authResponse.Token))
            throw new Exception($"Test setup failed: Could not log in as {role} ({email})");

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
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = role,
                LastName = "Test",
                IsActive = true
            };
            var result = await userManager.CreateAsync(user, ApiWebApplicationFactory.DefaultPassword);
            if (!result.Succeeded)
                throw new Exception($"Failed to seed {role} user: {string.Join("\n", result.Errors.Select(e => e.Description))}");
            
            result = await userManager.AddToRoleAsync(user, identityRole ?? role); // Add to specified identity role
             if (!result.Succeeded)
                throw new Exception($"Failed to add {role} user to role: {string.Join("\n", result.Errors.Select(e => e.Description))}");
        }
    }
    
     // Helper to create a dummy device for tests requiring an ID
    private async Task<int> CreateTestDeviceAsync(HttpClient? client = null)
    {
        client ??= await GetAuthenticatedClientAsync("Admin"); // Use admin by default
        var createDto = new CreateDeviceDto { Name = $"Test Device {Guid.NewGuid()}", OrderIndex = 1 };
        var response = await client.PostAsJsonAsync("/api/Device", createDto);
        response.EnsureSuccessStatusCode();
        var device = await response.Content.ReadFromJsonAsync<DeviceDto>();
        return device?.Id ?? throw new Exception("Failed to create test device");
    }

    // --- Endpoint Tests --- 

    // GET /api/Device
    [Theory]
    [InlineData("Admin", HttpStatusCode.OK)]
    [InlineData("Management", HttpStatusCode.OK)]
    [InlineData("User", HttpStatusCode.OK)]
    [InlineData("Debug", HttpStatusCode.OK)]
    public async Task GetAll_WithRequiredRole_ReturnsOk(string role, HttpStatusCode expectedStatus)
    {
        var client = await GetAuthenticatedClientAsync(role);
        var response = await client.GetAsync("/api/Device");
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task GetAll_Anonymous_ReturnsUnauthorized() => await TestAnonymous("/api/Device", HttpMethod.Get);

    // GET /api/Device/{id}
    [Theory]
    [InlineData("Admin", HttpStatusCode.OK)]
    [InlineData("Management", HttpStatusCode.OK)]
    [InlineData("User", HttpStatusCode.OK)]
    [InlineData("Debug", HttpStatusCode.OK)]
    public async Task GetById_WithRequiredRole_ReturnsOk(string role, HttpStatusCode expectedStatus)
    {
        var deviceId = await CreateTestDeviceAsync(); // Need a device to get
        var client = await GetAuthenticatedClientAsync(role);
        var response = await client.GetAsync($"/api/Device/{deviceId}");
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task GetById_Anonymous_ReturnsUnauthorized() => await TestAnonymous($"/api/Device/1", HttpMethod.Get); // Use dummy ID

    // POST /api/Device
    [Theory]
    [InlineData("Admin", HttpStatusCode.Created)]
    [InlineData("Management", HttpStatusCode.Created)]
    [InlineData("Debug", HttpStatusCode.Created)]
    [InlineData("User", HttpStatusCode.Forbidden)] // User cannot create
    public async Task Create_WithRole_ReturnsExpectedStatus(string role, HttpStatusCode expectedStatus)
    {
        var client = await GetAuthenticatedClientAsync(role);
        var createDto = new CreateDeviceDto { Name = $"Create Test {role}", OrderIndex = 1 };
        var response = await client.PostAsJsonAsync("/api/Device", createDto);
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task Create_Anonymous_ReturnsUnauthorized() => await TestAnonymous("/api/Device", HttpMethod.Post, new CreateDeviceDto());

    // PUT /api/Device/{id}
    [Theory]
    [InlineData("Admin", HttpStatusCode.NoContent)]
    [InlineData("Management", HttpStatusCode.NoContent)]
    [InlineData("Debug", HttpStatusCode.NoContent)]
    [InlineData("User", HttpStatusCode.Forbidden)] // User cannot update
    public async Task Update_WithRole_ReturnsExpectedStatus(string role, HttpStatusCode expectedStatus)
    {
        var deviceId = await CreateTestDeviceAsync();
        var client = await GetAuthenticatedClientAsync(role);
        var updateDto = new UpdateDeviceDto { Name = $"Update Test {role}", OrderIndex = 1 };
        var response = await client.PutAsJsonAsync($"/api/Device/{deviceId}", updateDto);
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task Update_Anonymous_ReturnsUnauthorized() => await TestAnonymous($"/api/Device/1", HttpMethod.Put, new UpdateDeviceDto());

    // DELETE /api/Device/{id}
    [Theory]
    [InlineData("Admin", HttpStatusCode.NoContent)]
    [InlineData("Debug", HttpStatusCode.NoContent)]
    [InlineData("Management", HttpStatusCode.Forbidden)] // Manager cannot delete
    [InlineData("User", HttpStatusCode.Forbidden)] // User cannot delete
    public async Task Delete_WithRole_ReturnsExpectedStatus(string role, HttpStatusCode expectedStatus)
    {
        // Use separate device for each delete attempt to avoid conflicts
        var deviceId = await CreateTestDeviceAsync(); 
        var client = await GetAuthenticatedClientAsync(role);
        var response = await client.DeleteAsync($"/api/Device/{deviceId}");
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task Delete_Anonymous_ReturnsUnauthorized() => await TestAnonymous($"/api/Device/1", HttpMethod.Delete);

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