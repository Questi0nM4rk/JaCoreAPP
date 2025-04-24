using System.Text.Json.Serialization;

namespace JaCore.Api.IntegrationTests.DTOs.Auth;

// DTO for integration tests, including JsonPropertyNames for serialization
public record TokenRefreshRequestDto(
    [property: JsonPropertyName("refreshToken")] string RefreshToken
);