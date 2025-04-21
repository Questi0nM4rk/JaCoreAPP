using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.User;

namespace JaCore.Api.Interfaces.Services;

/// <summary>
/// Interface for user management operations.
/// </summary>
public interface IUserService
{
    Task<PaginatedListDto<UserResponseDto>> GetAllUsersAsync(int pageNumber, int pageSize);
    Task<UserResponseDto?> GetUserByIdAsync(string id);
    Task<UserResponseDto> RegisterUserAsync(UserRegistrationDto registerDto);
    Task<bool> UpdateUserStatusAsync(string id, bool isActive);
    Task<bool> DeleteUserAsync(string id);
    // Note: Token revocation logic remains in AuthController as it directly uses UserManager and JWT service state.
} 