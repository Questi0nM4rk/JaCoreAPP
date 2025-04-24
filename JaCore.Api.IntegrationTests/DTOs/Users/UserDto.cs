using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JaCore.Api.IntegrationTests.DTOs.Users;

// DTO for integration tests, including JsonPropertyNames for deserialization
public record UserDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("isActive")] bool IsActive,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("roles")] IList<string> Roles
);