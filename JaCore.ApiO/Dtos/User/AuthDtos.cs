// DTOs/AuthDtos.cs
using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.DTOs;

public record RegisterDto(
    [Required, EmailAddress] string Email,
    [Required, MinLength(2)] string FirstName,
    [Required, MinLength(2)] string LastName,
    [Required, DataType(DataType.Password), MinLength(8)] string Password);

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required, DataType(DataType.Password)] string Password);

public record AuthResponseDto(
    bool Succeeded,
    string? Token = null,
    DateTime? Expiration = null,
    string? UserId = null,
    string? Email = null,
    IList<string>? Roles = null,
    string? Message = null);
