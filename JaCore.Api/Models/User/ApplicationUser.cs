using Microsoft.AspNetCore.Identity;

namespace JaCore.Api.Models.User;

/// <summary>
/// Application user model
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// First name of the user
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Last name of the user
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Full name of the user
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
    
    /// <summary>
    /// Flag to indicate if the user is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Refresh token for JWT authentication
    /// </summary>
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// Refresh token expiry date
    /// </summary>
    public DateTime RefreshTokenExpiryTime { get; set; }
    
    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime? LastModifiedAt { get; set; }
} 