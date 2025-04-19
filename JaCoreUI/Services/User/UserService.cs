using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JaCoreUI.Models.User;
using JaCoreUI.Services.Api;

namespace JaCoreUI.Services.User;

/// <summary>
/// Service for user-related operations
/// </summary>
public class UserService
{
    private readonly AuthService _authService;
    
    public Models.User.User? CurrentUser => _authService.CurrentUser;
    public bool IsAuthenticated => _authService.IsAuthenticated;
    
    public event EventHandler<Models.User.User?>? UserChanged;
    
    public UserService(AuthService authService)
    {
        _authService = authService;
    }
    
    /// <summary>
    /// Attempts to log in with the provided credentials
    /// </summary>
    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var user = await _authService.LoginAsync(email, password);
            UserChanged?.Invoke(this, user);
            return true;
        }
        catch (ApiException ex)
        {
            // Log the error
            Console.WriteLine($"Login failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Logs the current user out
    /// </summary>
    public async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        UserChanged?.Invoke(this, null);
    }
    
    /// <summary>
    /// Gets the current user's profile
    /// </summary>
    public async Task<Models.User.User?> GetCurrentUserAsync()
    {
        if (!_authService.IsAuthenticated)
        {
            return null;
        }
        
        try
        {
            var user = await _authService.GetUserProfileAsync();
            UserChanged?.Invoke(this, user);
            return user;
        }
        catch (ApiException ex)
        {
            // Log the error
            Console.WriteLine($"Failed to get user profile: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Checks if the current user has the specified role
    /// </summary>
    public bool HasRole(string role)
    {
        return _authService.CurrentUser?.Roles.Contains(role) ?? false;
    }
    
    /// <summary>
    /// Gets all users (admin only)
    /// </summary>
    public async Task<List<Models.User.User>?> GetAllUsersAsync()
    {
        if (!_authService.IsAuthenticated || !HasRole("Admin"))
        {
            return null;
        }
        
        try
        {
            return await _authService.GetAllUsersAsync();
        }
        catch (ApiException ex)
        {
            // Log the error
            Console.WriteLine($"Failed to get users: {ex.Message}");
            return null;
        }
    }
}