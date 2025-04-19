using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.Dtos.User;

/// <summary>
/// Data transfer object for user registration requests
/// </summary>
public class UserRegistrationDto
{
    /// <summary>
    /// Email address of the user
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Username for the account
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// User's first name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// User's last name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Password for the account
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Role for the new user (Admin, Debug, Management, User)
    /// </summary>
    [Required]
    public string Role { get; set; } = string.Empty;
} 