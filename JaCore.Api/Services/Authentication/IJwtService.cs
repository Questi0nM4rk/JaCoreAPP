using System.Security.Claims;
using JaCore.Api.Models.User;

namespace JaCore.Api.Services.Authentication;

/// <summary>
/// Interface for JWT operations - unified service that handles all JWT functionality
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generate JWT token for a user
    /// </summary>
    /// <param name="user">Application user</param>
    /// <param name="roles">User roles</param>
    /// <returns>JWT token string</returns>
    Task<string> GenerateToken(ApplicationUser user, IEnumerable<string> roles);
    
    /// <summary>
    /// Generate JWT token from claims
    /// </summary>
    /// <param name="claims">Claims to include in the token</param>
    /// <returns>JWT token string</returns>
    Task<string> GenerateAccessToken(IEnumerable<Claim> claims);

    /// <summary>
    /// Generate refresh token
    /// </summary>
    /// <returns>Refresh token string</returns>
    string GenerateRefreshToken();
    
    /// <summary>
    /// Validates a token
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool ValidateToken(string token);
    
    /// <summary>
    /// Get claims from a token
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>Claims principal</returns>
    ClaimsPrincipal GetClaimsFromToken(string token);

    /// <summary>
    /// Creates claims for a user
    /// </summary>
    /// <param name="user">The user to create claims for</param>
    /// <param name="roles">The user's roles</param>
    /// <returns>A collection of claims</returns>
    IEnumerable<Claim> CreateUserClaims(ApplicationUser user, IEnumerable<string> roles);
} 