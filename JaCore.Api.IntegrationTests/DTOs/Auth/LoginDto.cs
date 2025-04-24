using System.Text.Json.Serialization;

namespace JaCore.Api.IntegrationTests.DTOs.Auth;

// DTO for integration tests, including JsonPropertyNames for serialization
public record LoginDto
{
    [property: JsonPropertyName("email")] public required string Email { get; init; }
    [property: JsonPropertyName("password")] public required string Password { get; init; }
}