using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JaCore.Api.IntegrationTests.DTOs.Users;

// DTO for integration tests, including JsonPropertyNames for serialization
public record UpdateUserDto
{
    [property: JsonPropertyName("firstName")] public required string FirstName { get; init; }
    [property: JsonPropertyName("lastName")] public required string LastName { get; init; }
    [property: JsonPropertyName("email")] public required string Email { get; init; }
    [property: JsonPropertyName("isActive")] public bool? IsActive { get; init; } = null;
    [property: JsonPropertyName("roles")] public List<string>? Roles { get; init; } = null;
}