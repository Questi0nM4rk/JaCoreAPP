using JaCore.Api.DTOs.Auth;
using JaCore.Common; // Keep for Roles
using JaCore.Api.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens; // Needed for SecurityTokenException, SymmetricSecurityKey
using System; // Needed for Guid, Exception
using System.IdentityModel.Tokens.Jwt; // Needed for JwtSecurityTokenHandler, JwtSecurityToken
using System.Linq; // Needed for FirstOrDefault, Select
using System.Security.Claims;
using System.Text; // Needed for Encoding
using System.Threading.Tasks;
using JaCore.Api.Helpers; // Add using for the new constants

namespace JaCore.Api.Controllers.Auth;

[ApiController]
[ApiVersion(ApiConstants.Versions.V1_0_String)] // Use version string constant
[Route(ApiConstants.ApiRoutePrefix + "/auth")] // Use route prefix constant
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration; // Inject configuration for token decoding helper
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IConfiguration configuration, // Inject IConfiguration
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _configuration = configuration; // Assign configuration
        _logger = logger;
    }

    // Register and Login actions remain the same as in Response #47
    [HttpPost(ApiConstants.Routes.Register)] // Use route constant
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        _logger.LogInformation("Registration attempt for email {Email}.", registerDto.Email);

        try
        {
            var result = await _authService.RegisterAsync(registerDto);
            if (!result.Succeeded)
            {
                return BadRequest(new ProblemDetails { Title = "Registration Failed", Detail = result.Message });
            }
            _logger.LogInformation("Registration successful for email {Email}.", registerDto.Email);
            return Ok(result);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Unexpected error during registration for {Email}", registerDto.Email);
             return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Internal Server Error", Detail = "An unexpected error occurred during registration."});
        }
    }

    [HttpPost(ApiConstants.Routes.Login)] // Use route constant
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        _logger.LogInformation("Login attempt for email {Email}.", loginDto.Email);

         try
        {
            var result = await _authService.LoginAsync(loginDto);
            if (!result.Succeeded)
            {
                return Unauthorized(new ProblemDetails { Title = "Login Failed", Detail = result.Message });
            }
            _logger.LogInformation("Login successful for email {Email}.", loginDto.Email);
            return Ok(result);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Unexpected error during login for {Email}", loginDto.Email);
             return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Internal Server Error", Detail = "An unexpected error occurred during login."});
        }
    }


    // --- UPDATED Refresh Action ---
    [HttpPost(ApiConstants.Routes.Refresh)] // Use route constant
    [AllowAnonymous] // Refresh token is its own authentication
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Refresh(TokenRefreshRequestDto request)
    {
         _logger.LogInformation("Attempting token refresh.");

        // Try to get UserId from the (potentially expired) access token in the header
        if (!TryGetUserIdFromExpiredToken(out Guid userId))
        {
             // If we cannot get the UserID from the expired token, we cannot proceed with this strategy
            return Unauthorized(new ProblemDetails { Title = "Token Refresh Failed", Detail = "Could not identify user from accompanying token." });
        }

        try
        {
            // Pass the extracted userId and the refresh token string to the service
            var result = await _authService.RefreshAccessTokenAsync(request.RefreshToken, userId);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Token refresh failed for user {UserId}: {Message}", userId, result.Message);
                // Return 401 if refresh fails (invalid token, expired, revoked etc.)
                return Unauthorized(new ProblemDetails { Title = "Token Refresh Failed", Detail = result.Message });
            }

            _logger.LogInformation("Token refresh successful for user {UserId}.", result.UserId);
            return Ok(result); // Returns new AuthResponseDto with new tokens
        }
        catch (Exception ex) // Catch exceptions from the service layer
        {
            _logger.LogError(ex, "Unexpected error during token refresh for user {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Internal Server Error", Detail = "An unexpected error occurred during token refresh."});
        }
    }

    // --- UPDATED Logout Action ---
    [HttpPost(ApiConstants.Routes.Logout)] // Use route constant
    [Authorize] // User must be authenticated with a VALID access token to call logout
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout(TokenRefreshRequestDto request) // Client sends the RefreshToken to revoke
    {
        // Get the UserId from the *currently authenticated* user's claims (guaranteed valid by [Authorize])
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId))
        {
             _logger.LogError("Could not parse UserId {UserIdString} during logout for authenticated user.", userIdString);
             return BadRequest(new ProblemDetails { Title = "Logout Failed", Detail = "Invalid user context."});
        }

        _logger.LogInformation("Logout attempt for user {UserId}.", userId);

        try
        {
            // Pass the authenticated user's ID and the refresh token string to revoke
            var revoked = await _authService.RevokeRefreshTokenAsync(request.RefreshToken, userId);
            if (!revoked) _logger.LogWarning("Failed to revoke refresh token during logout for user {UserId}, it might have been invalid already.", userId);

            _logger.LogInformation("Logout successful for user {UserId}.", userId);
            return NoContent(); // Success, no content to return
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during logout for user {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Title = "Internal Server Error", Detail = "An unexpected error occurred during logout."});
        }
    }


    // GetCurrentUser and AdminOnlyData actions remain the same as in Response #47
    [HttpGet(ApiConstants.Routes.GetCurrentUser)] // Use route constant
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var firstName = User.FindFirstValue(ClaimTypes.GivenName);
        var lastName = User.FindFirstValue(ClaimTypes.Surname); // Or FamilyName
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        _logger.LogInformation("Accessed 'me' endpoint by user {UserId}.", userId);
        return Ok(new { UserId = userId, Email = email, FirstName = firstName, LastName = lastName, Roles = roles });
    }

    [HttpGet(ApiConstants.Routes.AdminOnlyData)] // Use route constant
    [Authorize(Policy = ApiConstants.Policies.AdminOnly)] // Use policy constant
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult AdminOnlyData()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Accessed 'admin-only' endpoint by admin user {UserId}.", userId);
        return Ok(new { Message = $"Welcome Admin User {userId}! This data is sensitive." });
    }


    // --- Private Helper Method to Extract UserId from Expired Token ---
    private bool TryGetUserIdFromExpiredToken(out Guid userId)
    {
        userId = Guid.Empty;
        // Attempt to get the token from the Authorization header
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) != true)
        {
            _logger.LogWarning("Refresh attempt failed: Missing or invalid Authorization header containing expired access token.");
            return false;
        }
        var token = authHeader.Substring("Bearer ".Length).Trim();

        // Get secret and validation parameters (disable lifetime validation)
        var jwtSecret = _configuration[ApiConstants.JwtConfigKeys.Secret]; // Use JWT key constant
         if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Length < 32) {
             _logger.LogError("JWT Secret not configured or too short, cannot decode token.");
             return false; // Cannot proceed without secret
         }

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true, // Keep other validations as needed
            ValidAudience = _configuration[ApiConstants.JwtConfigKeys.Audience], // Use JWT key constant
            ValidateIssuer = true,
            ValidIssuer = _configuration[ApiConstants.JwtConfigKeys.Issuer], // Use JWT key constant
            ValidateIssuerSigningKey = true, // Need key to read claims
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = false // *** IMPORTANT: Do not validate expiry time ***
        };

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            // ValidateToken checks signature/issuer/audience but ignores lifetime
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            // Optional: Check if the validated token is actually a JWT and uses the expected algorithm
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                 _logger.LogWarning("Refresh attempt failed: Invalid token format or algorithm in expired token.");
                 return false;
            }

            var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdClaim, out userId))
            {
                 _logger.LogDebug("Successfully extracted UserId {UserId} from expired token.", userId);
                return true; // Success
            }
            else
            {
                _logger.LogWarning("Refresh attempt failed: Could not parse user ID claim ('{ClaimValue}') from expired token.", userIdClaim);
                return false;
            }
        }
        catch (SecurityTokenException ex) // Catch specific token validation errors
        {
             _logger.LogWarning(ex, "Refresh attempt failed: SecurityTokenException while validating expired token (potentially invalid signature/issuer/audience).");
             return false;
        }
        catch (Exception ex) // Catch unexpected errors
        {
            _logger.LogError(ex, "Refresh attempt failed: Unexpected exception during expired token processing.");
            return false;
        }
    }
}
