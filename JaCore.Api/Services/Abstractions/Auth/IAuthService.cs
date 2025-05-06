using JaCore.Api.DTOs.Auth;
using System; // Needed for Guid
using System.Threading.Tasks;

namespace JaCore.Api.Services.Abstractions.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string defaultRole = "User");
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto> RefreshAccessTokenAsync(string refreshToken, Guid userId);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken, Guid userId);
}
