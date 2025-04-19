using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using JaCore.Api.Dtos.Device; // DTOs are in Device namespace
using JaCore.Api.Dtos.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using JaCore.Api.Models.User;

namespace JaCore.Api.IntegrationTests;

[Collection("Sequential")] 
public class ServiceControllerAuthTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _anonClient;

    public ServiceControllerAuthTests(ApiWebApplicationFactory factory)
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
    
    private async Task<int> CreateTestServiceAsync(HttpClient? client = null)
    {
        client ??= await GetAuthenticatedClientAsync("Admin");
        var createDto = new CreateServiceDto { Name = $"Test Service {Guid.NewGuid()}" };
        var response = await client.PostAsJsonAsync("/api/Service", createDto);
        response.EnsureSuccessStatusCode();
        var service = await response.Content.ReadFromJsonAsync<ServiceDto>();
        return service?.Id ?? throw new Exception("Failed to create test service");
    }

    // --- Endpoint Tests --- 

    // GET /api/Service
    [Theory]
    [InlineData("Admin", HttpStatusCode.OK)]
    [InlineData("Management", HttpStatusCode.OK)]
    [InlineData("User", HttpStatusCode.OK)]
    [InlineData("Debug", HttpStatusCode.OK)]
    public async Task GetAll_WithRequiredRole_ReturnsOk(string role, HttpStatusCode expectedStatus)
    {
        var client = await GetAuthenticatedClientAsync(role);
        var response = await client.GetAsync("/api/Service");
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task GetAll_Anonymous_ReturnsUnauthorized() => await TestAnonymous("/api/Service", HttpMethod.Get);

    // GET /api/Service/{id}
    [Theory]
    [InlineData("Admin", HttpStatusCode.OK)]
    [InlineData("Management", HttpStatusCode.OK)]
    [InlineData("User", HttpStatusCode.OK)]
    [InlineData("Debug", HttpStatusCode.OK)]
    public async Task GetById_WithRequiredRole_ReturnsOk(string role, HttpStatusCode expectedStatus)
    {
        var serviceId = await CreateTestServiceAsync();
        var client = await GetAuthenticatedClientAsync(role);
        var response = await client.GetAsync($"/api/Service/{serviceId}");
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task GetById_Anonymous_ReturnsUnauthorized() => await TestAnonymous($"/api/Service/1", HttpMethod.Get);

    // POST /api/Service
    [Theory]
    [InlineData("Admin", HttpStatusCode.Created)]
    [InlineData("Management", HttpStatusCode.Created)]
    [InlineData("Debug", HttpStatusCode.Created)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    public async Task Create_WithRole_ReturnsExpectedStatus(string role, HttpStatusCode expectedStatus)
    {
        var client = await GetAuthenticatedClientAsync(role);
        var createDto = new CreateServiceDto { Name = $"Create Test {role}" };
        var response = await client.PostAsJsonAsync("/api/Service", createDto);
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task Create_Anonymous_ReturnsUnauthorized() => await TestAnonymous("/api/Service", HttpMethod.Post, new CreateServiceDto());

    // PUT /api/Service/{id}
    [Theory]
    [InlineData("Admin", HttpStatusCode.NoContent)]
    [InlineData("Management", HttpStatusCode.NoContent)]
    [InlineData("Debug", HttpStatusCode.NoContent)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    public async Task Update_WithRole_ReturnsExpectedStatus(string role, HttpStatusCode expectedStatus)
    {
        var serviceId = await CreateTestServiceAsync();
        var client = await GetAuthenticatedClientAsync(role);
        var updateDto = new UpdateServiceDto { Name = $"Update Test {role}" };
        var response = await client.PutAsJsonAsync($"/api/Service/{serviceId}", updateDto);
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task Update_Anonymous_ReturnsUnauthorized() => await TestAnonymous($"/api/Service/1", HttpMethod.Put, new UpdateServiceDto());

    // DELETE /api/Service/{id}
    [Theory]
    [InlineData("Admin", HttpStatusCode.NoContent)]
    [InlineData("Debug", HttpStatusCode.NoContent)]
    [InlineData("Management", HttpStatusCode.Forbidden)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    public async Task Delete_WithRole_ReturnsExpectedStatus(string role, HttpStatusCode expectedStatus)
    {
        var serviceId = await CreateTestServiceAsync(); 
        var client = await GetAuthenticatedClientAsync(role);
        var response = await client.DeleteAsync($"/api/Service/{serviceId}");
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task Delete_Anonymous_ReturnsUnauthorized() => await TestAnonymous($"/api/Service/1", HttpMethod.Delete);

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