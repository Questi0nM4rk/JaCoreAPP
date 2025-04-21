using JaCore.Api.DTOs;
using JaCore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JaCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous] // Ensure registration is accessible without authentication
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        _logger.LogInformation("Registration attempt for email {Email}.", registerDto.Email);
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _authService.RegisterAsync(registerDto);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.Message ?? "Registration failed." });
            }

            _logger.LogInformation("Registration successful for email {Email}.", registerDto.Email);
            // Return the token and user info upon successful registration
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during registration for {Email}.", registerDto.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal error occurred." });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous] // Ensure login is accessible without authentication
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
         _logger.LogInformation("Login attempt for email {Email}.", loginDto.Email);
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _authService.LoginAsync(loginDto);

            if (!result.Succeeded)
            {
                // Return 401 for failed login attempts
                return Unauthorized(new { message = result.Message ?? "Login failed." });
            }

             _logger.LogInformation("Login successful for email {Email}.", loginDto.Email);
            return Ok(result);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "An unexpected error occurred during login for {Email}.", loginDto.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal error occurred." });
        }
    }

    // Example of a protected endpoint
    [HttpGet("me")]
    [Authorize] // Requires a valid JWT
    public IActionResult GetCurrentUser()
    {
        // Access user claims from the HttpContext
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Gets user ID (sub claim)
        var email = User.FindFirstValue(ClaimTypes.Email);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        _logger.LogInformation("Accessed 'me' endpoint by user {UserId}.", userId);

        return Ok(new { UserId = userId, Email = email, Roles = roles });
    }

    // Example of a role-protected endpoint
    [HttpGet("admin-only")]
    [Authorize(Roles = "Admin")] // Requires Admin role
    public IActionResult AdminOnlyData()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Accessed 'admin-only' endpoint by admin user {UserId}.", userId);
        return Ok(new { Message = $"Welcome Admin User {userId}! This data is sensitive." });
    }
}
 