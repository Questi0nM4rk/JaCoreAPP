using JaCore.Api.DTOs.Users;
using JaCore.Api.Entities.Identity;
using JaCore.Common;
using JaCore.Api.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JaCore.Api.Services.Users;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.AsNoTracking().ToListAsync();
        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(MapToUserDto(user, roles));
        }
        _logger.LogInformation("Retrieved {Count} users.", userDtos.Count);
        return userDtos;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) {
            _logger.LogWarning("User not found: {UserId}", id);
            return null;
        }
        var roles = await _userManager.GetRolesAsync(user);
        _logger.LogInformation("Retrieved user: {UserId}", id);
        return MapToUserDto(user, roles);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(Guid id, UpdateUserDto dto, Guid currentUserId, bool isAdmin)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return (false, "User not found.");

        // Authorization Check
        if (!isAdmin && user.Id != currentUserId)
        {
            _logger.LogWarning("Forbidden update attempt: User {CurrentUserId} tried to update {TargetUserId}", currentUserId, id);
            return (false, "Unauthorized to update this user.");
        }

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;

        // Handle Email Change
        if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Attempting email update for user {UserId} to {NewEmail}", id, dto.Email);
            var setEmailResult = await _userManager.SetEmailAsync(user, dto.Email);
            if (!setEmailResult.Succeeded) return (false, FormatErrors(setEmailResult));
             var setUserNameResult = await _userManager.SetUserNameAsync(user, dto.Email); // Keep username in sync
             if (!setUserNameResult.Succeeded) return (false, FormatErrors(setUserNameResult));
            // user.EmailConfirmed = false; // Require re-verification if enabled
        }

        // Admin-specific updates
        if (isAdmin)
        {
            // Update IsActive status and Lockout
            if (dto.IsActive.HasValue && user.IsActive != dto.IsActive.Value)
            {
                 user.IsActive = dto.IsActive.Value;
                 _logger.LogInformation("User {UserId} active status set to {IsActive} by admin {AdminUserId}.", id, user.IsActive, currentUserId);
                 if (!user.IsActive && user.LockoutEnd == null) await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                 else if (user.IsActive && user.LockoutEnd != null) await _userManager.SetLockoutEndDateAsync(user, null);
            }
            // Update Roles if provided in this DTO
            if (dto.Roles != null)
            {
                var roleResult = await UpdateUserRolesInternalAsync(user, dto.Roles);
                if (!roleResult.Success) return roleResult;
            }
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded) return (false, FormatErrors(updateResult));

        _logger.LogInformation("User updated successfully: {UserId}", id);
        return (true, null);
    }

    // Specifically for the admin endpoint to update only roles
    public async Task<(bool Success, string? ErrorMessage)> UpdateUserRolesAsync(Guid id, List<string> newRoleNames)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return (false, "User not found.");

        _logger.LogInformation("Admin attempting to update roles for user {UserId}", id);
        return await UpdateUserRolesInternalAsync(user, newRoleNames);
    }


    public async Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return (false, "User not found.");

        // --- Soft Delete ---
        if (!user.IsActive)
        {
            _logger.LogInformation("User {UserId} is already inactive.", id);
            return (true, null); // Or return a specific message indicating already inactive?
        }

        _logger.LogWarning("Attempting SOFT DELETE (setting IsActive=false) for user {UserId}", id);
        user.IsActive = false;
        // Optionally: Set LockoutEnd to prevent login immediately if IsActive check isn't enough
        // await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

        var result = await _userManager.UpdateAsync(user); // Update the user instead of deleting

        if (!result.Succeeded)
        {
             _logger.LogError("Soft delete failed for user {UserId}: {Errors}", id, FormatErrors(result));
             return (false, FormatErrors(result));
        }

        _logger.LogInformation("User soft deleted (marked inactive) successfully: {UserId}", id);
        return (true, null);
    }

    // --- Private Helper Methods ---
    private UserDto MapToUserDto(ApplicationUser user, IList<string> roles) =>
        new UserDto(user.Id, user.FirstName, user.LastName, user.Email ?? "", user.IsActive, user.CreatedAt, roles);

    private string FormatErrors(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(e => e.Description));

    // Common logic for updating roles
    private async Task<(bool Success, string? ErrorMessage)> UpdateUserRolesInternalAsync(ApplicationUser user, List<string> newRoleNames)
    {
        var distinctNewRoles = newRoleNames.Distinct().ToList(); // Work with distinct roles

        // Validate all new roles exist
        foreach (var roleName in distinctNewRoles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                _logger.LogError("Role update failed for user {UserId}: Role '{RoleName}' does not exist.", user.Id, roleName);
                return (false, $"Role '{roleName}' does not exist.");
            }
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.Except(distinctNewRoles).ToList();
        var rolesToAdd = distinctNewRoles.Except(currentRoles).ToList();

        IdentityResult removeResult = IdentityResult.Success;
        IdentityResult addResult = IdentityResult.Success;

        if (rolesToRemove.Any())
        {
            removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded) {
                 _logger.LogError("Failed removing roles [{Roles}] for user {UserId}: {Errors}", string.Join(',', rolesToRemove), user.Id, FormatErrors(removeResult));
                 return (false, FormatErrors(removeResult));
            }
             _logger.LogInformation("Removed roles [{Roles}] from user {UserId}.", string.Join(',', rolesToRemove), user.Id);
        }

        if (rolesToAdd.Any())
        {
            addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
             if (!addResult.Succeeded) {
                 _logger.LogError("Failed adding roles [{Roles}] for user {UserId}: {Errors}", string.Join(',', rolesToAdd), user.Id, FormatErrors(addResult));
                 return (false, FormatErrors(addResult));
             }
            _logger.LogInformation("Added roles [{Roles}] to user {UserId}.", string.Join(',', rolesToAdd), user.Id);
        }

        return (true, null);
    }
}
