using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JaCoreUI.Models.User;
using JaCoreUI.Services.Api;
using Microsoft.Extensions.Configuration;

namespace JaCoreUI.Services.User;

public class AuthService : ApiClientBase
{
    private string? _token;
    private string? _refreshToken;
    private DateTime _tokenExpiration = DateTime.MinValue;
    private Models.User.User? _currentUser;
    
    public bool IsAuthenticated => !string.IsNullOrEmpty(_token) && _tokenExpiration > DateTime.Now;
    public Models.User.User? CurrentUser => _currentUser;
    
    public AuthService(IConfiguration configuration) : base(configuration)
    {
    }
    
    /// <summary>
    /// Logs in a user with the provided credentials
    /// </summary>
    public async Task<Models.User.User> LoginAsync(string email, string password)
    {
        try
        {
            var loginData = new
            {
                Email = email,
                Password = password
            };
            
            var response = await PostAsync<object, AuthResponse>("Auth/login", loginData);
            
            if (!response.IsSuccess)
            {
                throw new ApiException(ApiErrorType.Unauthorized, response.ErrorMessage ?? "Login failed");
            }
            
            _token = response.Token;
            _refreshToken = response.RefreshToken;
            _tokenExpiration = response.Expiration;
            
            // Set the token for future API requests
            SetAuthToken(_token);
            
            // Create a user object from the response
            _currentUser = new Models.User.User
            {
                Username = response.User?.UserName ?? "Unknown",
                Email = response.User?.Email ?? email,
                Roles = response.User?.Role != null ? new List<string> { response.User.Role } : new List<string>()
            };
            
            return _currentUser;
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Login failed: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Refreshes the authentication token
    /// </summary>
    public async Task RefreshTokenAsync()
    {
        if (string.IsNullOrEmpty(_token) || string.IsNullOrEmpty(_refreshToken))
        {
            throw new ApiException(ApiErrorType.Unauthorized, "No token available to refresh");
        }
        
        try
        {
            var refreshData = new
            {
                AccessToken = _token,
                RefreshToken = _refreshToken
            };
            
            var response = await PostAsync<object, AuthResponse>("Auth/refresh-token", refreshData);
            
            if (!response.IsSuccess)
            {
                throw new ApiException(ApiErrorType.Unauthorized, response.ErrorMessage ?? "Token refresh failed");
            }
            
            _token = response.Token;
            _refreshToken = response.RefreshToken;
            _tokenExpiration = response.Expiration;
            
            // Set the token for future API requests
            SetAuthToken(_token);
            
            // Update the current user if available
            if (response.User != null)
            {
                _currentUser = new Models.User.User
                {
                    Username = response.User.UserName,
                    Email = response.User.Email,
                    Roles = response.User.Role != null ? new List<string> { response.User.Role } : new List<string>()
                };
            }
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Token refresh failed: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Logs out the current user
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            if (IsAuthenticated)
            {
                // Revoke the token on the server
                await PostAsync("Auth/revoke", new { });
            }
        }
        catch
        {
            // Ignore any errors when logging out
        }
        finally
        {
            // Clear local tokens and user
            _token = null;
            _refreshToken = null;
            _tokenExpiration = DateTime.MinValue;
            _currentUser = null;
            
            // Remove the token from future API requests
            SetAuthToken(string.Empty);
        }
    }
    
    /// <summary>
    /// Gets the current user's profile
    /// </summary>
    public async Task<Models.User.User> GetUserProfileAsync()
    {
        if (!IsAuthenticated)
        {
            throw new ApiException(ApiErrorType.Unauthorized, "User is not authenticated");
        }
        
        try
        {
            // Parse the token to get the user ID
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(_token) as JwtSecurityToken;
            var userId = jsonToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                throw new ApiException(ApiErrorType.Unauthorized, "Invalid token");
            }
            
            var userDto = await GetAsync<UserResponseDto>($"Auth/{userId}");
            
            _currentUser = new Models.User.User
            {
                Username = userDto.UserName,
                Email = userDto.Email,
                Roles = userDto.Role != null ? new List<string> { userDto.Role } : new List<string>()
            };
            
            return _currentUser;
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Failed to get user profile: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Gets all users (admin only)
    /// </summary>
    public async Task<List<Models.User.User>> GetAllUsersAsync()
    {
        if (!IsAuthenticated)
        {
            throw new ApiException(ApiErrorType.Unauthorized, "User is not authenticated");
        }
        
        try
        {
            var userDtos = await GetAsync<List<UserResponseDto>>("Auth");
            
            return userDtos.Select(dto => new Models.User.User
            {
                Username = dto.UserName,
                Email = dto.Email,
                Roles = dto.Role != null ? new List<string> { dto.Role } : new List<string>()
            }).ToList();
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Failed to get users: {ex.Message}", ex);
        }
    }
    
    #region DTOs
    
    private class AuthResponse
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public UserResponseDto? User { get; set; }
    }
    
    private class UserResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Role { get; set; }
        public bool IsActive { get; set; }
    }
    
    #endregion
} 