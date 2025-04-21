// Define the interface (optional but good practice)
using JaCore.Api.DTOs;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string defaultRole = "User");
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
}