using System.Text.Json.Serialization;

namespace JaCore.Api.IntegrationTests.DTOs.Auth;

// DTO for integration tests, including JsonPropertyNames for serialization
public record RegisterDto
{
    [property: JsonPropertyName("email")] public required string Email { get; init; }
    [property: JsonPropertyName("firstName")] public required string FirstName { get; init; }
    [property: JsonPropertyName("lastName")] public required string LastName { get; init; }
    [property: JsonPropertyName("password")] public required string Password { get; init; }
}