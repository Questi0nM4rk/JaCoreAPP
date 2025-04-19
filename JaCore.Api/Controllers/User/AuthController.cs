using System.IdentityModel.Tokens.Jwt;
using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.User;
using JaCore.Api.Interfaces.Services;
using JaCore.Api.Models.User;
using JaCore.Api.Services.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JaCore.Api.Controllers.User;

/// <summary>
/// Controller for user authentication and management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;
    
    /// <summary>
    /// Constructor for AuthController
    /// </summary>
    public AuthController(
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService,
        IUserService userService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _userService = userService;
        _logger = logger;
    }
    
    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>AuthResponse with token on success</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login(UserLoginDto loginDto)
    {
        _logger.LogInformation("Login attempt for user: {Email}", loginDto.Email);
        
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            _logger.LogWarning("Invalid login attempt for email: {Email}", loginDto.Email);
            return Unauthorized(new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Invalid credentials"
            });
        }
        
        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for deactivated account: {Email}", loginDto.Email);
            return Unauthorized(new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Account is deactivated"
            });
        }
        
        var userRoles = await _userManager.GetRolesAsync(user);
        
        // Generate tokens
        var token = await _jwtService.GenerateToken(user, userRoles);
        var refreshToken = _jwtService.GenerateRefreshToken();
        
        // Save refresh token to user
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // TODO: Use config for validity
        await _userManager.UpdateAsync(user);
        
        _logger.LogInformation("User {Email} logged in successfully", user.Email);
        
        var userDto = new UserResponseDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = userRoles.FirstOrDefault() ?? string.Empty,
            IsActive = user.IsActive
        };
        
        return Ok(new AuthResponseDto
        {
            IsSuccess = true,
            Token = token,
            RefreshToken = refreshToken,
            User = userDto
        });
    }
    
    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="registerDto">Registration details</param>
    /// <returns>Success message on successful registration</returns>
    [HttpPost("register")]
    [Authorize(Roles = "Admin,Debug")] // Only admin or debug can register new users
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponseDto>> Register(UserRegistrationDto registerDto)
    {
        try
        {
            var createdUser = await _userService.RegisterUserAsync(registerDto);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(ex.ParamName ?? "Error", ex.Message);
            return BadRequest(ModelState);
        }
        catch (InvalidOperationException ex) // Catch potential Identity errors
        {
            ModelState.AddModelError("Registration", ex.Message);
            return BadRequest(ModelState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user {Email}.", registerDto.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred during registration.");
        }
    }
    
    /// <summary>
    /// Get a specific user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Debug,Management")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> GetUserById(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        
        if (user == null)
        {
            return NotFound();
        }
        
        return Ok(user);
    }
    
    /// <summary>
    /// Get all users (with optional filtering)
    /// </summary>
    /// <returns>List of users</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Debug,Management")]
    [ProducesResponseType(typeof(PaginatedListDto<UserResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedListDto<UserResponseDto>>> GetAllUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;
        
        var users = await _userService.GetAllUsersAsync(pageNumber, pageSize);
        return Ok(users);
    }
    
    /// <summary>
    /// Update user's active status
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="statusDto">DTO containing the active status</param>
    /// <returns>No content on success</returns>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Debug")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserStatus(string id, [FromBody] UpdateUserStatusDto statusDto)
    {
        var success = await _userService.UpdateUserStatusAsync(id, statusDto.IsActive);
        if (!success)
        {
            var exists = await _userService.GetUserByIdAsync(id) != null;
            return exists ? StatusCode(StatusCodes.Status500InternalServerError, "Failed to update user status.") : NotFound();
        }
        return NoContent();
    }
    
    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Debug")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var success = await _userService.DeleteUserAsync(id);
        if (!success)
        {
            var exists = await _userService.GetUserByIdAsync(id) != null;
            return exists ? StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete user.") : NotFound();
        }
        return NoContent();
    }
    
    /// <summary>
    /// Refresh an access token using a refresh token
    /// </summary>
    /// <param name="tokenModel">Token model with access and refresh tokens</param>
    /// <returns>New tokens on success</returns>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] TokenModel tokenModel)
    {
        if (tokenModel is null || string.IsNullOrEmpty(tokenModel.AccessToken) || string.IsNullOrEmpty(tokenModel.RefreshToken))
        {
            return BadRequest("Invalid client request");
        }

        ClaimsPrincipal principal;
        try
        {
            principal = _jwtService.GetClaimsFromToken(tokenModel.AccessToken);
        }
        catch (Exception ex) // Catch specific exceptions if needed
        {
            _logger.LogWarning(ex, "Invalid access token provided for refresh.");
            return BadRequest(new AuthResponseDto { IsSuccess = false, ErrorMessage = "Invalid access token" });
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new AuthResponseDto { IsSuccess = false, ErrorMessage = "Invalid token claims" });
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null || user.RefreshToken != tokenModel.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            _logger.LogWarning("Invalid refresh token for user {UserId}", userId);
            return Unauthorized(new AuthResponseDto { IsSuccess = false, ErrorMessage = "Invalid client request" });
        }
        
        if (!user.IsActive)
        {
            _logger.LogWarning("Refresh token attempt for deactivated user {UserId}", userId);
            return Unauthorized(new AuthResponseDto { IsSuccess = false, ErrorMessage = "Account is deactivated" });
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        var newAccessToken = await _jwtService.GenerateToken(user, userRoles);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // TODO: Use config
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Token refreshed successfully for user {UserId}", userId);
        
        var userDto = new UserResponseDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = userRoles.FirstOrDefault() ?? string.Empty,
            IsActive = user.IsActive
        };

        return Ok(new AuthResponseDto
        {
            IsSuccess = true,
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            User = userDto
        });
    }
    
    /// <summary>
    /// Revoke the refresh token for the current user
    /// </summary>
    /// <returns>No content on success</returns>
    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Revoke()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("Invalid token claims");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return BadRequest(); // Or NotFound?

        user.RefreshToken = null; // Clear the refresh token
        user.RefreshTokenExpiryTime = DateTime.MinValue;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Refresh token revoked for user {UserId}", userId);
        return NoContent();
    }

    /// <summary>
    /// Revoke all refresh tokens (only available to admins)
    /// </summary>
    /// <returns>No content on success</returns>
    [HttpPost("revoke-all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeAll()
    {
        var users = _userManager.Users.ToList();
        foreach (var user in users)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = DateTime.MinValue;
            await _userManager.UpdateAsync(user);
        }
        _logger.LogInformation("All refresh tokens revoked by admin.");
        return NoContent();
    }
} 