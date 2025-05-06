using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace JaCore.Api.DTOs.Auth;

// Convert from class to nominal record
public record RegisterDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(2)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MinLength(2)]
    public string LastName { get; init; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(10)]
    public string Password { get; init; } = string.Empty;

    [Required]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; init; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "At least one role must be specified.")]
    public IList<string> Roles { get; init; } = new List<string>();

    public string? PhoneNumber { get; init; } // Optional property
}
