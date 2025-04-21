using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.User;
using JaCore.Api.Interfaces.Services;
using JaCore.Api.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JaCore.Api.Services.User;

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

    public async Task<PaginatedListDto<UserResponseDto>> GetAllUsersAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;

        var totalCount = await _userManager.Users.CountAsync();
        var users = await _userManager.Users
            .OrderBy(u => u.UserName) // Or other default sort
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userDtos = new List<UserResponseDto>();
        foreach (var user in users)
        {
            userDtos.Add(await MapToUserResponseDto(user));
        }
        
        return new PaginatedListDto<UserResponseDto>(userDtos, totalCount, pageNumber, pageSize);
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        return user == null ? null : await MapToUserResponseDto(user);
    }

    public async Task<UserResponseDto> RegisterUserAsync(UserRegistrationDto registerDto)
    {
        // Validation
        if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
        {
            throw new ArgumentException("Email is already taken", nameof(registerDto.Email));
        }
        if (await _userManager.FindByNameAsync(registerDto.UserName) != null)
        {
            throw new ArgumentException("Username is already taken", nameof(registerDto.UserName));
        }
        if (!await _roleManager.RoleExistsAsync(registerDto.Role))
        {
            throw new ArgumentException($"Role '{registerDto.Role}' does not exist", nameof(registerDto.Role));
        }

        var user = new ApplicationUser
        {
            UserName = registerDto.UserName,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("User registration failed for {Email}: {Errors}", registerDto.Email, errors);
            // Throw a more generic exception or a custom one
            throw new InvalidOperationException($"User registration failed: {errors}");
        }

        // Assign role
        var roleResult = await _userManager.AddToRoleAsync(user, registerDto.Role);
        if (!roleResult.Succeeded)
        {
            // Log error, but maybe don't fail registration? Or attempt cleanup?
             _logger.LogWarning("Failed to assign role '{Role}' to user {Email} after registration.", registerDto.Role, registerDto.Email);
             // For now, proceed but the user won't have the role.
        }

        return await MapToUserResponseDto(user);
    }

    public async Task<bool> UpdateUserStatusAsync(string id, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return false;
        }

        user.IsActive = isActive;
        user.LastModifiedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed to update status for user {UserId}: {Errors}", id, string.Join(", ", result.Errors.Select(e=>e.Description)));
            // Potentially throw or return specific error info
        }
        return result.Succeeded;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return false; // Or true if idempotent delete desired
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
             _logger.LogError("Failed to delete user {UserId}: {Errors}", id, string.Join(", ", result.Errors.Select(e=>e.Description)));
        }
        return result.Succeeded;
    }

    // Helper to map ApplicationUser to UserResponseDto
    private async Task<UserResponseDto> MapToUserResponseDto(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new UserResponseDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = roles.FirstOrDefault() ?? string.Empty,
            IsActive = user.IsActive
        };
    }
} 