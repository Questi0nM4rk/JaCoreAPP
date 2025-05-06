using AutoMapper;
using JaCore.Api.Data;
using JaCore.Api.Services.Abstractions.Users;
using JaCore.Api.Entities.Identity;
using JaCore.Api.DTOs.Users;
using JaCore.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;

namespace JaCore.Api.Services.Users;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<UserService> _logger;
    private readonly IMapper _mapper;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<UserService> logger,
        IMapper mapper)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var userDtos = await _userManager.Users
            .AsNoTracking()
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

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
        
        var userDto = _mapper.Map<UserDto>(user);

        // userDto.Roles = await _userManager.GetRolesAsync(user); // Error CS8852: Cannot assign to init-only property.
        // TODO: Roles need to be populated via AutoMapper profile configuration (e.g., using AfterMap or a custom resolver).

        _logger.LogInformation("Retrieved user: {UserId}", id);
        return userDto;
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(Guid id, UpdateUserDto dto, Guid currentUserId, bool isAdmin)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return (false, "User not found.");

        if (!isAdmin && user.Id != currentUserId)
        {
            _logger.LogWarning("Forbidden update attempt: User {CurrentUserId} tried to update {TargetUserId}", currentUserId, id);
            return (false, "Unauthorized to update this user.");
        }

        _mapper.Map(dto, user); 

        if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
             _logger.LogInformation("Attempting email update for user {UserId} to {NewEmail}", id, dto.Email);
             var existingUserWithNewEmail = await _userManager.FindByEmailAsync(dto.Email);
             if (existingUserWithNewEmail != null && existingUserWithNewEmail.Id != user.Id)
             {
                 _logger.LogWarning("Email update for user {UserId} failed: New email {NewEmail} is already in use.", id, dto.Email);
                 return (false, "Email address is already in use.");
             }

             var setEmailResult = await _userManager.SetEmailAsync(user, dto.Email);
             if (!setEmailResult.Succeeded) return (false, FormatErrors(setEmailResult));
              var setUserNameResult = await _userManager.SetUserNameAsync(user, dto.Email);
              if (!setUserNameResult.Succeeded) {
                   _logger.LogError("Failed to sync username after email update for user {UserId}. Username may be out of sync.", id);
              }
             user.EmailConfirmed = false;
             _logger.LogInformation("User {UserId} email updated to {NewEmail} and marked as unconfirmed.", id, dto.Email);
        }

        if (isAdmin)
        {
            if (dto.IsActive.HasValue && user.IsActive != dto.IsActive.Value)
            {
                 user.IsActive = dto.IsActive.Value;
                 _logger.LogInformation("User {UserId} active status set to {IsActive} by admin {AdminUserId}.", id, user.IsActive, currentUserId);
                 if (!user.IsActive && user.LockoutEnd == null) await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                 else if (user.IsActive && user.LockoutEnd != null) await _userManager.SetLockoutEndDateAsync(user, null);
            }
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
        if (!user.IsActive) return (true, null);
        user.IsActive = false;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) { /* log */ return (false, FormatErrors(result)); }
        /* log */ return (true, null);
    }

    private string FormatErrors(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(e => e.Description));

    private async Task<(bool Success, string? ErrorMessage)> UpdateUserRolesInternalAsync(ApplicationUser user, List<string> newRoleNames)
    {
       var distinctNewRoles = newRoleNames.Distinct().ToList();
       foreach (var roleName in distinctNewRoles) { if (!await _roleManager.RoleExistsAsync(roleName)) return (false, $"Role '{roleName}' does not exist."); }
       var currentRoles = await _userManager.GetRolesAsync(user);
       var rolesToRemove = currentRoles.Except(distinctNewRoles).ToList();
       var rolesToAdd = distinctNewRoles.Except(currentRoles).ToList();
       if (rolesToRemove.Any()) { var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove); if (!removeResult.Succeeded) { /* log */ return (false, FormatErrors(removeResult)); } /* log */ }
       if (rolesToAdd.Any()) { var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd); if (!addResult.Succeeded) { /* log */ return (false, FormatErrors(addResult)); } /* log */ }
       return (true, null);
    }
}
