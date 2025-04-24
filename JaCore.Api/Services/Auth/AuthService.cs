using JaCore.Api.Data; // For DbContext (Unit of Work)
using JaCore.Api.DTOs.Auth;
using JaCore.Api.Entities.Auth;
using JaCore.Api.Entities.Identity;
using JaCore.Common; // Keep for Roles
using JaCore.Api.Services.Abstractions; // Use Interfaces
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Required for Include/ToListAsync potentially
using JaCore.Api.Helpers; // Add using for the new constants

namespace JaCore.Api.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IRefreshTokenRepository _refreshTokenRepository; // Use Repository Interface
    private readonly ApplicationDbContext _context; // Keep for SaveChangesAsync (Unit of Work)
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly IPasswordHasher<RefreshToken> _refreshTokenHasher;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IRefreshTokenRepository refreshTokenRepository, // Inject Repository
        ApplicationDbContext context, // Inject DbContext for UoW
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _refreshTokenRepository = refreshTokenRepository;
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _refreshTokenHasher = new PasswordHasher<RefreshToken>();
    }

    // RegisterAsync and LoginAsync remain the same as in Response #45
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string defaultRole = RoleConstants.Roles.User) // Use RENAMED RoleConstants
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed: Email {Email} already exists.", registerDto.Email);
            return new AuthResponseDto(false, Message: "Email already exists.");
        }

        var newUser = new ApplicationUser
        {
            FirstName = registerDto.FirstName, LastName = registerDto.LastName, Email = registerDto.Email, UserName = registerDto.Email, IsActive = true
        };

        var result = await _userManager.CreateAsync(newUser, registerDto.Password); // Hashes password
        if (!result.Succeeded)
        {
            var errors = FormatErrors(result);
            _logger.LogError("User creation failed for {Email}: {Errors}", registerDto.Email, errors);
            return new AuthResponseDto(false, Message: $"User creation failed: {errors}");
        }
        _logger.LogInformation("User {Email} created successfully.", registerDto.Email);

        // Add to default role
        if (!string.IsNullOrEmpty(defaultRole))
        {
             if (!await _roleManager.RoleExistsAsync(defaultRole))
            {
                 _logger.LogWarning("Default role '{Role}' not found. Creating it.", defaultRole);
                 await _roleManager.CreateAsync(new ApplicationRole(defaultRole) { Description = $"{defaultRole} role created automatically."});
            }
            var roleResult = await _userManager.AddToRoleAsync(newUser, defaultRole);
            if (!roleResult.Succeeded) _logger.LogError("Failed to add user {Email} to role {Role}: {Errors}", registerDto.Email, defaultRole, FormatErrors(roleResult));
            else _logger.LogInformation("User {Email} added to role {Role}.", registerDto.Email, defaultRole);
        }

        var tokenResponse = await GenerateTokensAsync(newUser);
        await _context.SaveChangesAsync(); // Commit user creation and new refresh token
        _logger.LogInformation("User {UserId} registered and initial tokens saved.", newUser.Id);
        return tokenResponse;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
         var user = await _userManager.FindByEmailAsync(loginDto.Email);
         if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
         {
             _logger.LogWarning("Login failed for {Email}: Invalid credentials.", loginDto.Email);
             return new AuthResponseDto(false, Message: "Invalid email or password.");
         }
         // *** ADDED CHECK: Prevent login if user is inactive ***
         if (!user.IsActive)
         {
              _logger.LogWarning("Login failed for {Email}: Account inactive.", loginDto.Email);
              return new AuthResponseDto(false, Message: "Account is inactive.");
         }

        _logger.LogInformation("User {Email} logged in successfully.", loginDto.Email);
        var tokenResponse = await GenerateTokensAsync(user);
        await _context.SaveChangesAsync(); // Commit the new refresh token
        _logger.LogInformation("New refresh token saved for user {UserId}.", user.Id);
        return tokenResponse;
    }

    // --- REFACTORED RefreshAccessTokenAsync ---
    // ** Now requires the userId associated with the tokens **
    public async Task<AuthResponseDto> RefreshAccessTokenAsync(string refreshToken, Guid userId)
    {
        _logger.LogDebug("Attempting token refresh for user {UserId}.", userId);

        // 1. Fetch VALID candidate tokens for THIS user using the repository
        var validUserTokens = await _refreshTokenRepository.GetValidTokensByUserIdAsync(userId);
        if (!validUserTokens.Any())
        {
            _logger.LogWarning("No valid refresh tokens found for user {UserId} during refresh attempt.", userId);
            // It's possible the token belonged to this user but was already revoked/used/expired
            return new AuthResponseDto(false, Message: "Invalid refresh token or session expired.");
        }

        // 2. Find the specific token by verifying the hash
        RefreshToken? storedToken = null;
        foreach (var candidate in validUserTokens)
        {
            // Compare hash of received token with stored hash
            if (_refreshTokenHasher.VerifyHashedPassword(candidate, candidate.TokenHash, refreshToken) == PasswordVerificationResult.Success)
            {
                storedToken = candidate;
                break; // Found the match
            }
        }

        // 3. Validate Found Token
        if (storedToken == null)
        {
            _logger.LogWarning("Refresh token validation failed for user {UserId}: Token hash mismatch or token invalid.", userId);
            // Security: Consider revoking all other valid tokens for this user if an invalid one belonging to them is used.
            // await RevokeAllUserTokensAsync(userId); // Needs implementation
            return new AuthResponseDto(false, Message: "Invalid refresh token.");
        }

        // 4. Rotation: Mark old token as used
        storedToken.IsUsed = true;
        await _refreshTokenRepository.UpdateAsync(storedToken); // Queue update

        // 5. Get user for new token generation
        // Since we already queried by UserId, we primarily need to ensure the user still exists and is active.
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if(user == null) {
             // This case means the user was deleted after the token was issued but before it expired/was used.
             _logger.LogError("User {UserId} for valid refresh token {TokenId} not found during refresh.", storedToken.UserId, storedToken.Id);
             storedToken.IsRevoked = true; // Clean up the now orphaned token
             await _context.SaveChangesAsync(); // Save revoke status
             return new AuthResponseDto(false, Message: "Associated user not found.");
        }
         // *** ADDED CHECK: Prevent refresh if user is inactive ***
         if (!user.IsActive) {
              _logger.LogWarning("User {UserId} is inactive, cannot refresh token.", storedToken.UserId);
              storedToken.IsRevoked = true; // Revoke the token if user becomes inactive
              await _context.SaveChangesAsync();
              return new AuthResponseDto(false, Message: "User account is inactive.");
         }

        _logger.LogInformation("Refresh token validated for user {UserId}. Rotating token.", storedToken.UserId);

        // 6. Generate new tokens (includes AddAsync via repo for the NEW refresh token)
        var newTokenResponse = await GenerateTokensAsync(user);

        // 7. Save changes (marks old token used, adds new token)
        try
        {
             await _context.SaveChangesAsync(); // Commit transaction
             _logger.LogInformation("Rotated refresh token and saved changes for user {UserId}.", storedToken.UserId);
             return newTokenResponse;
        }
        catch(DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Failed to save changes during token rotation for user {UserId}", storedToken.UserId);
            // If saving fails, the client still has the old (now potentially invalid if IsUsed was key) refresh token.
            // The state is inconsistent. This indicates a potential DB issue.
            return new AuthResponseDto(false, Message:"Failed to update token state. Please try logging in again.");
        }
    }

    // --- REFACTORED RevokeRefreshTokenAsync ---
    // ** Now requires the userId associated with the token **
    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, Guid userId)
    {
        _logger.LogDebug("Attempting token revocation for user {UserId}.", userId);

        // 1. Fetch valid candidate tokens for this user
        var validUserTokens = await _refreshTokenRepository.GetValidTokensByUserIdAsync(userId);

        // 2. Find the specific token by verifying the hash
        RefreshToken? storedToken = null;
        foreach (var candidate in validUserTokens)
        {
            if (_refreshTokenHasher.VerifyHashedPassword(candidate, candidate.TokenHash, refreshToken) == PasswordVerificationResult.Success)
            {
                storedToken = candidate;
                break;
            }
        }

        // 3. Validate Found Token
        if (storedToken == null)
        {
            _logger.LogWarning("Revoke failed for user {UserId}: Refresh token not found or already invalid.", userId);
            return false; // Indicate token wasn't found or already invalid
        }

        // 4. Mark as revoked
        storedToken.IsRevoked = true;
        await _refreshTokenRepository.UpdateAsync(storedToken); // Queue update

        // 5. Save the change
        try
        {
            await _context.SaveChangesAsync(); // Commit transaction
            _logger.LogInformation("Refresh token {TokenId} revoked for user {UserId}.", storedToken.Id, storedToken.UserId);
            return true;
        }
        catch(DbUpdateException dbEx)
        {
             _logger.LogError(dbEx, "Failed to save revoke status for token {TokenId}, user {UserId}", storedToken.Id, storedToken.UserId);
             return false; // Indicate failure to save
        }
    }

    // --- Private Helper Methods ---
    // GenerateTokensAsync, GenerateClaims, GenerateRefreshTokenString, FormatErrors
    // remain the same as in Response #47
    private async Task<AuthResponseDto> GenerateTokensAsync(ApplicationUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);
        var claims = GenerateClaims(user, userRoles);

        var jwtSecret = _configuration[ApiConstants.JwtConfigKeys.Secret]!; // Use new constant
        var issuer = _configuration[ApiConstants.JwtConfigKeys.Issuer]; // Use new constant
        var audience = _configuration[ApiConstants.JwtConfigKeys.Audience]; // Use new constant
        var accessExpiryMinutes = _configuration.GetValue<int>(ApiConstants.JwtConfigKeys.AccessExpiryMinutes, 15); // Use new constant
        var refreshExpiryDays = _configuration.GetValue<int>(ApiConstants.JwtConfigKeys.RefreshExpiryDays, 7); // Use new constant

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Access Token
        var accessExpires = DateTime.UtcNow.AddMinutes(accessExpiryMinutes);
        var accessTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = accessExpires,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var accessToken = tokenHandler.CreateToken(accessTokenDescriptor);
        var accessTokenString = tokenHandler.WriteToken(accessToken);

        // Refresh Token
        var refreshTokenString = GenerateRefreshTokenString();
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _refreshTokenHasher.HashPassword(null!, refreshTokenString),
            ExpiryDate = DateTime.UtcNow.AddDays(refreshExpiryDays),
            CreatedDate = DateTime.UtcNow,
            IsRevoked = false,
            IsUsed = false
        };
        await _refreshTokenRepository.AddAsync(refreshTokenEntity); // Add via repo
        // SaveChangesAsync called by the calling public method

        _logger.LogDebug("Generated tokens; refresh token queued for add for user {UserId}.", user.Id);
        // Use Common Role constants if needed in AuthResponseDto constructor (assuming it takes roles)
        return new AuthResponseDto(true, accessTokenString, accessExpires, refreshTokenString, user.Id.ToString(), user.Email, user.FirstName, user.LastName, Roles:userRoles, Message: "Login successful.");
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
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private string FormatErrors(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(e => e.Description));

    // Optional Security Enhancement (Example)
    // private async Task RevokeAllUserTokensAsync(Guid userId)
    // {
    //     _logger.LogWarning("Revoking all valid refresh tokens for user {UserId} due to potential security issue.", userId);
    //     var tokens = await _refreshTokenRepository.GetValidTokensByUserIdAsync(userId);
    //     if (tokens.Any())
    //     {
    //         foreach (var token in tokens) { token.IsRevoked = true; await _refreshTokenRepository.UpdateAsync(token); }
    //         await _context.SaveChangesAsync();
    //         _logger.LogInformation("Successfully revoked {Count} tokens for user {UserId}.", tokens.Count, userId);
    //     }
    // }
}
