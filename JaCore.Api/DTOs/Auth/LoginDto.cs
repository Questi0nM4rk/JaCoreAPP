using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.DTOs.Auth;

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required, DataType(DataType.Password)] string Password
);
