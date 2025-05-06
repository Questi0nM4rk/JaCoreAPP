using JaCore.Api.Data; // For DbContext (Unit of Work)
using JaCore.Api.DTOs.Auth;
using JaCore.Api.Entities.Auth;
using JaCore.Api.Entities.Identity;
using JaCore.Common; // Keep for Roles
using JaCore.Api.Services.Abstractions.Auth; // Use Interfaces
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
using AutoMapper; // Added for IMapper
using JaCore.Api.Services.Repositories;

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
    private readonly IMapper _mapper; // Added IMapper

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IRefreshTokenRepository refreshTokenRepository, // Inject Repository
        ApplicationDbContext context, // Inject DbContext for UoW
        IConfiguration configuration,
        ILogger<AuthService> logger,
        IMapper mapper) // Inject IMapper
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _refreshTokenRepository = refreshTokenRepository;
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _refreshTokenHasher = new PasswordHasher<RefreshToken>();
        _mapper = mapper; // Assign IMapper
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string defaultRole)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed: Email {Email} already exists.", registerDto.Email);
            return new AuthResponseDto(false, Message: "Email already exists.");
        }

        // Use AutoMapper to map DTO to Entity
        var newUser = _mapper.Map<ApplicationUser>(registerDto);
        // Note: Specific fields ignored in the profile (like Id, PasswordHash) are handled correctly.
        // UserName is mapped from Email, IsActive is set to true by the profile.

        var result = await _userManager.CreateAsync(newUser, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = FormatErrors(result);
            _logger.LogError("User creation failed for {Email}: {Errors}", registerDto.Email, errors);
            return new AuthResponseDto(false, Message: $"User creation failed: {errors}");
        }
        _logger.LogInformation("User {Email} created successfully with ID {UserId}.", registerDto.Email, newUser.Id);

        if (registerDto.Roles == null || !registerDto.Roles.Any())
        {
             _logger.LogError("User {Email} created but no roles were provided in registration request. Deleting user.", registerDto.Email);
             await _userManager.DeleteAsync(newUser);
             return new AuthResponseDto(false, Message: "Registration failed: At least one role must be provided.");
        }

        var validRolesToAdd = new List<string>();
        foreach (var roleName in registerDto.Roles.Distinct())
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                 _logger.LogError("User {Email} registration failed: Invalid role \'{RoleName}\' provided. Deleting user.", registerDto.Email, roleName);
                 await _userManager.DeleteAsync(newUser);
                 return new AuthResponseDto(false, Message: $"Unknown role specified: {roleName}");
            }
            validRolesToAdd.Add(roleName);
        }

        if (!validRolesToAdd.Any())
        {
            _logger.LogError("User {Email} created but no valid roles were identified (this should not happen if validation passed). Deleting user.", 
                registerDto.Email);
            await _userManager.DeleteAsync(newUser);
            return new AuthResponseDto(false, Message: "Registration failed: No valid roles were processed.");
        }

        var roleResult = await _userManager.AddToRolesAsync(newUser, validRolesToAdd);
        if (!roleResult.Succeeded)
        {
            var roleErrors = FormatErrors(roleResult);
            _logger.LogError("Failed to add user {Email} to roles [{Roles}]: {Errors}. Deleting user.", 
                registerDto.Email, string.Join(", ", validRolesToAdd), roleErrors);
            await _userManager.DeleteAsync(newUser);
            return new AuthResponseDto(false, Message: $"Failed to assign roles: {roleErrors}");
        }
        
        _logger.LogInformation("User {Email} added to roles [{Roles}].", registerDto.Email, string.Join(", ", validRolesToAdd));

        var tokenResponse = await GenerateTokensAsync(newUser);
        await _context.SaveChangesAsync();
        _logger.LogInformation("User {UserId} registered, roles assigned, and initial tokens saved.", newUser.Id);

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
         if (!user.IsActive)
         {
              _logger.LogWarning("Login failed for {Email}: Account inactive.", loginDto.Email);
              return new AuthResponseDto(false, Message: "Account is inactive.");
         }

        _logger.LogInformation("User {Email} logged in successfully.", loginDto.Email);
        var tokenResponse = await GenerateTokensAsync(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation("New refresh token saved for user {UserId}.", user.Id);
        return tokenResponse;
    }

    public async Task<AuthResponseDto> RefreshAccessTokenAsync(string refreshToken, Guid userId)
    {
        _logger.LogDebug("Attempting token refresh for user {UserId}.", userId);

        var validUserTokens = await _refreshTokenRepository.GetValidTokensByUserIdAsync(userId);
        if (!validUserTokens.Any())
        {
            _logger.LogWarning("No valid refresh tokens found for user {UserId} during refresh attempt.", userId);
            return new AuthResponseDto(false, Message: "Invalid refresh token or session expired.");
        }

        RefreshToken? storedToken = null;
        foreach (var candidate in validUserTokens)
        {
            if (_refreshTokenHasher.VerifyHashedPassword(candidate, candidate.TokenHash, refreshToken) == PasswordVerificationResult.Success)
            {
                storedToken = candidate;
                break;
            }
        }

        if (storedToken == null)
        {
            _logger.LogWarning("Refresh token validation failed for user {UserId}: Token hash mismatch or token invalid.", userId);
            return new AuthResponseDto(false, Message: "Invalid refresh token.");
        }

        storedToken.IsUsed = true;
        await _refreshTokenRepository.UpdateAsync(storedToken);

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if(user == null) {
             _logger.LogError("User {UserId} for valid refresh token {TokenId} not found during refresh.", storedToken.UserId, storedToken.Id);
             storedToken.IsRevoked = true;
             await _context.SaveChangesAsync();
             return new AuthResponseDto(false, Message: "Associated user not found.");
        }
         if (!user.IsActive) {
              _logger.LogWarning("User {UserId} is inactive, cannot refresh token.", storedToken.UserId);
              storedToken.IsRevoked = true;
              await _context.SaveChangesAsync();
              return new AuthResponseDto(false, Message: "User account is inactive.");
         }

        _logger.LogInformation("Refresh token validated for user {UserId}. Rotating token.", storedToken.UserId);

        var newTokenResponse = await GenerateTokensAsync(user);

        try
        {
             await _context.SaveChangesAsync();
             _logger.LogInformation("Rotated refresh token and saved changes for user {UserId}.", storedToken.UserId);
             return newTokenResponse;
        }
        catch(DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Failed to save changes during token rotation for user {UserId}", storedToken.UserId);
            return new AuthResponseDto(false, Message:"Failed to update token state. Please try logging in again.");
        }
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, Guid userId)
    {
        _logger.LogDebug("Attempting token revocation for user {UserId}.", userId);

        var validUserTokens = await _refreshTokenRepository.GetValidTokensByUserIdAsync(userId);

        RefreshToken? storedToken = null;
        foreach (var candidate in validUserTokens)
        {
            if (_refreshTokenHasher.VerifyHashedPassword(candidate, candidate.TokenHash, refreshToken) == PasswordVerificationResult.Success)
            {
                storedToken = candidate;
                break;
            }
        }

        if (storedToken == null)
        {
            _logger.LogWarning("Revoke failed for user {UserId}: Refresh token not found or already invalid.", userId);
            return false;
        }

        storedToken.IsRevoked = true;
        await _refreshTokenRepository.UpdateAsync(storedToken);

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Refresh token {TokenId} revoked for user {UserId}.", storedToken.Id, storedToken.UserId);
            return true;
        }
        catch(DbUpdateException dbEx)
        {
             _logger.LogError(dbEx, "Failed to save revoke status for token {TokenId}, user {UserId}", storedToken.Id, storedToken.UserId);
             return false;
        }
    }

    private async Task<AuthResponseDto> GenerateTokensAsync(ApplicationUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);
        var claims = GenerateClaims(user, userRoles);

        var jwtConfig = _configuration.GetSection(ApiConstants.JwtConfigKeys.Section);
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig[ApiConstants.JwtConfigKeys.Secret] 
            ?? throw new InvalidOperationException("JWT Secret not configured")));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var accessTokenExpirationMinutes = double.Parse(jwtConfig[ApiConstants.JwtConfigKeys.AccessExpiryMinutes] ?? "15");
        var accessToken = new JwtSecurityToken(
            issuer: jwtConfig[ApiConstants.JwtConfigKeys.Issuer],
            audience: jwtConfig[ApiConstants.JwtConfigKeys.Audience],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes),
            signingCredentials: credentials);

        var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);

        var refreshTokenString = GenerateRefreshTokenString();
        var refreshTokenExpiryDays = double.Parse(jwtConfig[ApiConstants.JwtConfigKeys.RefreshExpiryDays] ?? "7");

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _refreshTokenHasher.HashPassword(null!, refreshTokenString),
            ExpiryDate = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
            CreatedDate = DateTime.UtcNow,
            IsUsed = false,
            IsRevoked = false,
        };

        await _refreshTokenRepository.AddAsync(refreshToken);

        _logger.LogDebug("Generated new access and refresh tokens for user {UserId}", user.Id);

        return new AuthResponseDto(
            Succeeded: true,
            AccessToken: accessTokenString,
            AccessTokenExpiration: accessToken.ValidTo,
            RefreshToken: refreshTokenString,
            UserId: user.Id.ToString(),
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Roles: await _userManager.GetRolesAsync(user)
        );
    }

    private List<Claim> GenerateClaims(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
            new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new Claim("isActive", user.IsActive.ToString(), ClaimValueTypes.Boolean)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

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
        string.Join(" ", result.Errors.Select(e => e.Description));
}
