namespace JaCore.Api.Dtos.User;

/// <summary>
/// Authentication response DTO
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// Indicates if the authentication was successful
    /// </summary>
    public bool IsSuccess { get; set; } = true;
    
    /// <summary>
    /// JWT token for authenticated requests
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// Refresh token for getting new JWT tokens
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Error message if authentication failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// User details for successful authentication
    /// </summary>
    public UserResponseDto? User { get; set; }
} 