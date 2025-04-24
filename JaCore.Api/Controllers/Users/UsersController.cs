using JaCore.Api.DTOs.Users;
using JaCore.Api.Services.Abstractions;
using JaCore.Common; // Keep for Roles
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using JaCore.Api.Helpers; // Add using for the new constants

namespace JaCore.Api.Controllers.Users;

[ApiController]
[ApiVersion(ApiConstants.Versions.V1_0_String)] // Use version string constant
[Route(ApiConstants.BasePaths.Users)] // Use route prefix constant
[Authorize] // Require authentication for all user actions by default
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // GET: api/v1/users
    [HttpGet]
    [Authorize(Policy = ApiConstants.Policies.AdminOnly)] // Use policy constant
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers()
    {
        _logger.LogInformation("Admin requested list of all users.");
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    // GET: api/v1/users/{id}
    [HttpGet(ApiConstants.UserEndpoints.GetById)] // Use route constant
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole(RoleConstants.Roles.Admin); // Use RENAMED RoleConstants

        // Allow admin to get anyone, users to get only themselves
        if (!isAdmin && currentUserId != id)
        {
            _logger.LogWarning("Forbidden attempt by user {CurrentUserId} to access user {TargetUserId} details.", currentUserId, id);
            return Forbid();
        }

        _logger.LogInformation("Request to get user {TargetUserId} by user {CurrentUserId}.", id, currentUserId);
        var user = await _userService.GetUserByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    // GET: api/v1/users/me
    [HttpGet(ApiConstants.UserEndpoints.Me)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            _logger.LogWarning("Could not parse user ID from claims for /me endpoint.");
            // This shouldn't happen for an authenticated user, but handle defensively
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = "Invalid user identifier." });
        }

        _logger.LogInformation("Request for current user data by user {UserId}.", userId);
        var user = await _userService.GetUserByIdAsync(userId);

        return user == null
            ? NotFound(new ProblemDetails { Title = "Not Found", Detail = "Current user data not found." })
            : Ok(user);
    }

    // PUT: api/v1/users/{id}
    [HttpPut(ApiConstants.UserEndpoints.GetById)] // Use route constant
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto updateUserDto)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole(RoleConstants.Roles.Admin);

        _logger.LogInformation("Attempt to update user {TargetUserId} by user {CurrentUserId}.", id, currentUserId);
        var (success, errorMessage) = await _userService.UpdateUserAsync(id, updateUserDto, currentUserId, isAdmin);

        if (!success)
        {
            if (errorMessage == "User not found.") return NotFound(new ProblemDetails { Title = "Not Found", Detail = errorMessage });
            if (errorMessage == "Unauthorized to update this user.") return Forbid();
            return BadRequest(new ProblemDetails { Title = "Update Failed", Detail = errorMessage });
        }
        return NoContent();
    }

    // PUT: api/v1/users/{id}/roles
    [HttpPut(ApiConstants.UserEndpoints.UpdateRoles)] // Use route constant
    [Authorize(Policy = ApiConstants.Policies.AdminOnly)] // Use policy constant
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserRoles(Guid id, [FromBody] UpdateUserRolesDto updateUserRolesDto)
    {
         var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
         _logger.LogInformation("Admin {AdminUserId} attempting to update roles for user {TargetUserId}.", currentUserId, id);
         var (success, errorMessage) = await _userService.UpdateUserRolesAsync(id, updateUserRolesDto.Roles);

         if (!success)
         {
             if (errorMessage == "User not found.") return NotFound(new ProblemDetails { Title = "Not Found", Detail = errorMessage });
             return BadRequest(new ProblemDetails { Title = "Role Update Failed", Detail = errorMessage });
         }
         return NoContent();
    }

    // DELETE: api/v1/users/{id}
    [HttpDelete(ApiConstants.UserEndpoints.GetById)] // Use route constant
    [Authorize(Policy = ApiConstants.Policies.AdminOnly)] // Use policy constant
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Admin {AdminUserId} attempting to delete user {TargetUserId}.", currentUserId, id);
        var (success, errorMessage) = await _userService.DeleteUserAsync(id);

        if (!success)
        {
            if (errorMessage == "User not found.") return NotFound(new ProblemDetails { Title = "Not Found", Detail = errorMessage });
            return BadRequest(new ProblemDetails { Title = "Delete Failed", Detail = errorMessage });
        }
        return NoContent();
    }
}
