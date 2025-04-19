using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.Dtos.User;

/// <summary>
/// Data transfer object for user login requests
/// </summary>
public class UserLoginDto
{
    /// <summary>
    /// Email address for login
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Password for login
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
} 