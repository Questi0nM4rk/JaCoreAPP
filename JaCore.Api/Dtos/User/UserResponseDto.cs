namespace JaCore.Api.Dtos.User;

/// <summary>
/// User response DTO
/// </summary>
public class UserResponseDto
{
    /// <summary>
    /// User ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Username
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// First name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// User role
    /// </summary>
    public string Role { get; set; } = string.Empty;
    
    /// <summary>
    /// Is user active
    /// </summary>
    public bool IsActive { get; set; } = true;
} 