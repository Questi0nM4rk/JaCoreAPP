using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using JaCore.Api.Dtos;
using JaCore.Api.Dtos.Device; // DTOs are in Device namespace
using JaCore.Api.Dtos.User; // Corrected namespace from Auth to User
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using JaCore.Api.Models.User;
using JaCore.Common.Device; // For EventType
using Xunit;

namespace JaCore.Api.IntegrationTests;

[Collection("Sequential")] 
public class EventControllerAuthTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _anonClient;

    public EventControllerAuthTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        try
        {
            _anonClient = _factory.CreateClient();
        }
        catch (Exception ex)
        {
            // Log or print the *full* exception details, including the InnerException
            Console.WriteLine("----- HOST BUILD FAILED IN EventControllerAuthTests -----");
            Console.WriteLine(ex.ToString()); 
            // If InnerException exists, it often holds the actual configuration error
            if (ex.InnerException != null)
            {
                Console.WriteLine("----- INNER EXCEPTION -----");
                Console.WriteLine(ex.InnerException.ToString());
            }
            Console.WriteLine("------------------------------------------------------");
            throw; // Re-throw so the test still fails
        }
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
        var createDto = new CreateDeviceDto { Name = $"DeviceForEventCard {Guid.NewGuid()}", OrderIndex = 1 };
        var response = await client.PostAsJsonAsync("/api/Device", createDto);
        response.EnsureSuccessStatusCode();
        var device = await response.Content.ReadFromJsonAsync<DeviceDto>();
        return device?.Id ?? throw new Exception("Failed to create test device for event tests");
    }

    private async Task<int> CreateTestDeviceCardAsync(HttpClient? adminClient = null)
    {
        adminClient ??= await GetAuthenticatedClientAsync("Admin");
        var deviceId = await CreateTestDeviceAsync(adminClient);
        var createDto = new CreateDeviceCardDto { SerialNumber = $"TESTCARD-EVT-{Guid.NewGuid()}" };
        var response = await adminClient.PostAsJsonAsync($"/api/DeviceCard/ForDevice/{deviceId}", createDto);
        // Handle potential conflict if card already exists (though unlikely with GUID)
        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
             // Attempt to get the existing card's ID if possible, or handle differently
             // This part depends on how the API returns conflicts
             // For now, re-throw or fail test clearly
             throw new Exception("Conflict creating device card, possibly due to previous test run state.");
        }
        response.EnsureSuccessStatusCode();
        var card = await response.Content.ReadFromJsonAsync<DeviceCardDto>();
        return card?.Id ?? throw new Exception("Failed to create test device card for event tests");
    }

    private async Task<int> CreateTestEventAsync(int deviceCardId, HttpClient? client = null)
    {
        client ??= await GetAuthenticatedClientAsync("Admin");
        var createDto = new CreateEventDto 
        { 
            DeviceCardId = deviceCardId, 
            Type = EventType.Service, 
            Description = $"Test Event {Guid.NewGuid()}" 
        };
        var response = await client.PostAsJsonAsync("/api/Event", createDto);
        response.EnsureSuccessStatusCode();
        var ev = await response.Content.ReadFromJsonAsync<EventDto>();
        return ev?.Id ?? throw new Exception("Failed to create test event");
    }

    // --- Endpoint Tests --- 

    // GET /api/Event?deviceCardId={id}
    [Theory]
    [InlineData("Admin", HttpStatusCode.OK)]
    [InlineData("Management", HttpStatusCode.OK)]
    [InlineData("User", HttpStatusCode.OK)]
    [InlineData("Debug", HttpStatusCode.OK)]
    public async Task GetByDeviceCardId_WithRequiredRole_ReturnsOk(string role, HttpStatusCode expectedStatus)
    {
        var cardId = await CreateTestDeviceCardAsync(); 
        var client = await GetAuthenticatedClientAsync(role);
        var response = await client.GetAsync($"/api/Event?deviceCardId={cardId}");
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task GetByDeviceCardId_Anonymous_ReturnsUnauthorized() => await TestAnonymous($"/api/Event?deviceCardId=1", HttpMethod.Get);
    [Fact] public async Task GetByDeviceCardId_NoCardId_ReturnsBadRequest() 
    { 
        var client = await GetAuthenticatedClientAsync("User");
        var response = await client.GetAsync("/api/Event");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // GET /api/Event/{id}
    [Theory]
    [InlineData("Admin", HttpStatusCode.OK)]
    [InlineData("Management", HttpStatusCode.OK)]
    [InlineData("User", HttpStatusCode.OK)]
    [InlineData("Debug", HttpStatusCode.OK)]
    public async Task GetById_WithRequiredRole_ReturnsOk(string role, HttpStatusCode expectedStatus)
    {
        var cardId = await CreateTestDeviceCardAsync();
        var eventId = await CreateTestEventAsync(cardId);
        var client = await GetAuthenticatedClientAsync(role);
        var response = await client.GetAsync($"/api/Event/{eventId}");
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task GetById_Anonymous_ReturnsUnauthorized() => await TestAnonymous($"/api/Event/1", HttpMethod.Get);

    // POST /api/Event
    [Theory]
    [InlineData("Admin", HttpStatusCode.Created)]
    [InlineData("Management", HttpStatusCode.Created)]
    [InlineData("Debug", HttpStatusCode.Created)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    public async Task Create_WithRole_ReturnsExpectedStatus(string role, HttpStatusCode expectedStatus)
    {
        var cardId = await CreateTestDeviceCardAsync();
        var client = await GetAuthenticatedClientAsync(role);
        var createDto = new CreateEventDto { DeviceCardId = cardId, Description = $"Create Event {role}" };
        var response = await client.PostAsJsonAsync("/api/Event", createDto);
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task Create_Anonymous_ReturnsUnauthorized() => await TestAnonymous("/api/Event", HttpMethod.Post, new CreateEventDto { DeviceCardId = 1});
    [Fact] public async Task Create_NoCardId_ReturnsBadRequest()
    { 
        var client = await GetAuthenticatedClientAsync("Admin");
        var createDto = new CreateEventDto { Description = "No Card ID" };
        var response = await client.PostAsJsonAsync("/api/Event", createDto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }


    // PUT /api/Event/{id}
    [Theory]
    [InlineData("Admin", HttpStatusCode.NoContent)]
    [InlineData("Management", HttpStatusCode.NoContent)]
    [InlineData("Debug", HttpStatusCode.NoContent)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    public async Task Update_WithRole_ReturnsExpectedStatus(string role, HttpStatusCode expectedStatus)
    {
        var cardId = await CreateTestDeviceCardAsync();
        var eventId = await CreateTestEventAsync(cardId);
        var client = await GetAuthenticatedClientAsync(role);
        var updateDto = new UpdateEventDto { Description = $"Update Test {role}" };
        var response = await client.PutAsJsonAsync($"/api/Event/{eventId}", updateDto);
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task Update_Anonymous_ReturnsUnauthorized() => await TestAnonymous($"/api/Event/1", HttpMethod.Put, new UpdateEventDto());

    // DELETE /api/Event/{id}
    [Theory]
    [InlineData("Admin", HttpStatusCode.NoContent)]
    [InlineData("Debug", HttpStatusCode.NoContent)]
    [InlineData("Management", HttpStatusCode.Forbidden)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    public async Task Delete_WithRole_ReturnsExpectedStatus(string role, HttpStatusCode expectedStatus)
    {
        var cardId = await CreateTestDeviceCardAsync();
        var eventId = await CreateTestEventAsync(cardId); 
        var client = await GetAuthenticatedClientAsync(role);
        var response = await client.DeleteAsync($"/api/Event/{eventId}");
        Assert.Equal(expectedStatus, response.StatusCode);
    }
    [Fact] public async Task Delete_Anonymous_ReturnsUnauthorized() => await TestAnonymous($"/api/Event/1", HttpMethod.Delete);

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