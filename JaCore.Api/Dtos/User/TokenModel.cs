using System.Text.Json.Serialization;

namespace JaCore.Api.Dtos.User;

/// <summary>
/// Model for token refresh requests
/// </summary>
public class TokenModel
{
    /// <summary>
    /// The access token (JWT)
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// The refresh token
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
} 