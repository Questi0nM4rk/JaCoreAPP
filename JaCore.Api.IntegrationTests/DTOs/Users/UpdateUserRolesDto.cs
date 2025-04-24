using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JaCore.Api.IntegrationTests.DTOs.Users;

// DTO for integration tests, including JsonPropertyNames for serialization
public record UpdateUserRolesDto
{
    [property: JsonPropertyName("roles")] public required List<string> Roles { get; init; }
}