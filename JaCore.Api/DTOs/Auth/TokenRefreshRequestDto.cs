using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.DTOs.Auth;

public record TokenRefreshRequestDto(
    [Required] string RefreshToken // Client sends the raw refresh token
);
