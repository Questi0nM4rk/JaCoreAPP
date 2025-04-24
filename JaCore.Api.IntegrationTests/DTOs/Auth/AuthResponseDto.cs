using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JaCore.Api.IntegrationTests.DTOs.Auth;

// DTO for integration tests, including JsonPropertyNames for deserialization
public record AuthResponseDto(
    [property: JsonPropertyName("succeeded")] bool Succeeded,
    [property: JsonPropertyName("accessToken")] string? AccessToken = null,
    [property: JsonPropertyName("accessTokenExpiration")] DateTime? AccessTokenExpiration = null,
    [property: JsonPropertyName("refreshToken")] string? RefreshToken = null,
    [property: JsonPropertyName("userId")] string? UserId = null,
    [property: JsonPropertyName("email")] string? Email = null,
    [property: JsonPropertyName("firstName")] string? FirstName = null,
    [property: JsonPropertyName("lastName")] string? LastName = null,
    [property: JsonPropertyName("roles")] IList<string>? Roles = null,
    [property: JsonPropertyName("message")] string? Message = null
);