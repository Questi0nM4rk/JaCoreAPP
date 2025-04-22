using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.DTOs.Auth;

public record RegisterDto(
    [Required, EmailAddress] string Email,
    [Required, MinLength(2)] string FirstName,
    [Required, MinLength(2)] string LastName,
    [Required, DataType(DataType.Password), MinLength(10)] string Password // Match Identity min length
);
