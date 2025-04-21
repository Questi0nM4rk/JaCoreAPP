namespace JaCore.Api.Models.Auth;

/// <summary>
/// JWT settings for the application
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// JWT issuer
    /// </summary>
    public string ValidIssuer { get; set; } = string.Empty;
    
    /// <summary>
    /// JWT audience
    /// </summary>
    public string ValidAudience { get; set; } = string.Empty;
    
    /// <summary>
    /// JWT secret key
    /// </summary>
    public string Secret { get; set; } = string.Empty;
    
    /// <summary>
    /// Token validity in minutes
    /// </summary>
    public int TokenValidityInMinutes { get; set; } = 180;
    
    /// <summary>
    /// Refresh token validity in days
    /// </summary>
    public int RefreshTokenValidityInDays { get; set; } = 7;
    
    /// <summary>
    /// Validate audience
    /// </summary>
    public bool ValidateAudience { get; set; } = true;
    
    /// <summary>
    /// Validate issuer
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;
} 