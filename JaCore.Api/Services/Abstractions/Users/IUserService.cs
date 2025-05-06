using JaCore.Api.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JaCore.Api.Services.Abstractions.Users;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto, Guid currentUserId, bool isAdmin);
    Task<(bool Success, string? ErrorMessage)> UpdateUserRolesAsync(Guid id, List<string> newRoleNames); // Admin only endpoint target
    Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(Guid id);
}
