using System.Collections.Generic;

namespace JaCore.Api.DTOs.Auth;

public record AuthResponseDto(
    bool Succeeded,
    string? AccessToken = null,
    DateTime? AccessTokenExpiration = null,
    string? RefreshToken = null, // The raw refresh token string (sent once)
    string? UserId = null,
    string? Email = null,
    string? FirstName = null,
    string? LastName = null,
    IList<string>? Roles = null,
    string? Message = null
);
