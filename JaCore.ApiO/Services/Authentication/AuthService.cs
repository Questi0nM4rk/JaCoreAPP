// Services/AuthService.cs
using JaCore.Api.Data;
using JaCore.Api.DTOs;
using JaCore.Api.Entities;
using JaCore.Api.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // Required for DB operations
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography; // For generating refresh token
using System.Text;
using System.Threading.Tasks;

namespace JaCore.Api.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _context; // Inject DbContext for refresh tokens
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly IPasswordHasher<RefreshToken> _refreshTokenHasher; // Inject hasher for refresh tokens

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ApplicationDbContext context, // Inject DbContext
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _configuration = configuration;
        _logger = logger;
        // Use a suitable hasher (PasswordHasher works fine for comparing hashes)
        _refreshTokenHasher = new PasswordHasher<RefreshToken>();
    }

    // RegisterAsync remains largely the same, but calls GenerateTokensAsync
     public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string defaultRole = "User")
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration attempt failed: Email {Email} already exists.", registerDto.Email);
            return new AuthResponseDto(false, Message: "Email already exists.");
        }

        var newUser = new ApplicationUser
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            UserName = registerDto.Email,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(newUser, registerDto.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("User creation failed for {Email}: {Errors}", registerDto.Email, errors);
            return new AuthResponseDto(false, Message: $"User creation failed: {errors}");
        }

        _logger.LogInformation("User {Email} created successfully.", registerDto.Email);

        // Add user to the default role
        if (!string.IsNullOrEmpty(defaultRole))
        {
             if (!await _roleManager.RoleExistsAsync(defaultRole))
            {
                _logger.LogWarning("Default role '{Role}' not found. Creating it.", defaultRole);
                await _roleManager.CreateAsync(new ApplicationRole(defaultRole) { Description = $"{defaultRole} role created automatically." });
            }
            var roleResult = await _userManager.AddToRoleAsync(newUser, defaultRole);
             if (!roleResult.Succeeded)
            {
                var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to add user {Email} to role {Role}: {Errors}", registerDto.Email, defaultRole, roleErrors);
            }
            else
            {
                _logger.LogInformation("User {Email} added to role {Role}.", registerDto.Email, defaultRole);
            }
        }

        // Generate tokens upon successful registration
        return await GenerateTokensAsync(newUser);
    }

    // LoginAsync remains largely the same, but calls GenerateTokensAsync
    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            _logger.LogWarning("Login failed for email {Email}: Invalid credentials.", loginDto.Email);
            return new AuthResponseDto(false, Message: "Invalid email or password.");
        }

        if (!user.IsActive)
        {
             _logger.LogWarning("Login failed for email {Email}: Account is inactive.", loginDto.Email);
            return new AuthResponseDto(false, Message: "Account is inactive.");
        }

        _logger.LogInformation("User {Email} logged in successfully.", loginDto.Email);
        return await GenerateTokensAsync(user);
    }

    // NEW: Refresh Token Logic
    public async Task<AuthResponseDto> RefreshAccessTokenAsync(string refreshToken)
    {
        // Find the stored refresh token (hashed)
        // We cannot directly query by the token string, as it's hashed.
        // Strategy: Find potential tokens by expiry, then verify hash.
        // OR: Include a non-sensitive identifier if needed, but adds complexity.
        // For simplicity here, let's assume we *could* find the user associated somehow if needed,
        // but the core is verifying the token itself against stored hashes.
        // A better approach involves finding the token by a unique ID or user association if possible.

        // Let's refine: Find token candidates first
        var potentialTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiryDate >= DateTime.UtcNow && !rt.IsRevoked && !rt.IsUsed)
            .ToListAsync(); // Get candidates that might match

        RefreshToken? storedToken = null;
        ApplicationUser? user = null;

        foreach (var candidate in potentialTokens)
        {
            var verificationResult = _refreshTokenHasher.VerifyHashedPassword(candidate, candidate.TokenHash, refreshToken);
            if (verificationResult == PasswordVerificationResult.Success)
            {
                storedToken = candidate;
                user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
                break; // Found the matching token
            }
        }

        // Validate the found token
        if (storedToken == null || user == null)
        {
            _logger.LogWarning("Refresh token validation failed: Token not found or user missing.");
            return new AuthResponseDto(false, Message: "Invalid refresh token.");
        }

        if (storedToken.ExpiryDate < DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token validation failed: Token expired for user {UserId}.", storedToken.UserId);
            storedToken.IsRevoked = true; // Mark expired token as revoked
            await _context.SaveChangesAsync();
            return new AuthResponseDto(false, Message: "Refresh token expired.");
        }

        if (storedToken.IsRevoked || storedToken.IsUsed)
        {
            _logger.LogWarning("Refresh token validation failed: Token revoked or already used for user {UserId}.", storedToken.UserId);
            // Optionally revoke all tokens for this user if a used/revoked token is attempted again (security measure)
            return new AuthResponseDto(false, Message: "Refresh token invalid.");
        }

        // Mark the current refresh token as used (part of rotation)
        storedToken.IsUsed = true;
        _context.RefreshTokens.Update(storedToken);
        await _context.SaveChangesAsync(); // Save change immediately

        _logger.LogInformation("Refresh token validated successfully for user {UserId}. Generating new tokens.", user.Id);

        // Generate new access and refresh tokens
        return await GenerateTokensAsync(user);
    }


    // Helper to generate both tokens and save the refresh token
    private async Task<AuthResponseDto> GenerateTokensAsync(ApplicationUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);
        var claims = GenerateClaims(user, userRoles);

        var jwtSecret = _configuration["Jwt:Secret"];
        var jwtIssuer = _configuration["Jwt:Issuer"];
        var jwtAudience = _configuration["Jwt:Audience"];
        var accessExpiryMinutes = _configuration.GetValue<int>("Jwt:AccessExpiryMinutes", 15); // Shorter lifespan for access token
        var refreshExpiryDays = _configuration.GetValue<int>("Jwt:RefreshExpiryDays", 7);   // Longer lifespan for refresh token

        if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Length < 32)
        {
             _logger.LogCritical("JWT Secret configuration error.");
            return new AuthResponseDto(false, Message: "Server configuration error.");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // --- Generate Access Token ---
        var accessTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(accessExpiryMinutes),
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            SigningCredentials = credentials
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var accessToken = tokenHandler.CreateToken(accessTokenDescriptor);
        var accessTokenString = tokenHandler.WriteToken(accessToken);

        // --- Generate and Save Refresh Token ---
        var refreshTokenString = GenerateRefreshTokenString();
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _refreshTokenHasher.HashPassword(null!, refreshTokenString), // Hash the token
            ExpiryDate = DateTime.UtcNow.AddDays(refreshExpiryDays),
            CreatedDate = DateTime.UtcNow,
            IsRevoked = false,
            IsUsed = false
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync(); // Save the new refresh token

        _logger.LogInformation("Generated new access and refresh tokens for user {UserId}.", user.Id);

        return new AuthResponseDto(
            Succeeded: true,
            AccessToken: accessTokenString,
            AccessTokenExpiration: accessTokenDescriptor.Expires,
            RefreshToken: refreshTokenString, // Send the raw token string to the client
            UserId: user.Id.ToString(),
            Email: user.Email,
            Roles: userRoles);
    }

    private List<Claim> GenerateClaims(ApplicationUser user, IList<string> roles)
    {
         var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        return claims;
    }

    private string GenerateRefreshTokenString()
    {
        var randomNumber = new byte[64]; // Increase size for more entropy
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

     // Optional: Method to revoke tokens, e.g., on logout
    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
        // Similar logic to finding the token in RefreshAccessTokenAsync
         var potentialTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiryDate >= DateTime.UtcNow && !rt.IsRevoked && !rt.IsUsed)
            .ToListAsync();

        RefreshToken? storedToken = null;
         foreach (var candidate in potentialTokens)
        {
            var verificationResult = _refreshTokenHasher.VerifyHashedPassword(candidate, candidate.TokenHash, refreshToken);
            if (verificationResult == PasswordVerificationResult.Success)
            {
                storedToken = candidate;
                break;
            }
        }

        if (storedToken is null) return false; // Token not found or invalid

        storedToken.IsRevoked = true;
        _context.RefreshTokens.Update(storedToken);
        await _context.SaveChangesAsync();
         _logger.LogInformation("Refresh token revoked for user {UserId}.", storedToken.UserId);
        return true;
    }
}

// UPDATE Interface
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string defaultRole = "User");
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto> RefreshAccessTokenAsync(string refreshToken);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken); // Optional revoke method
}
