using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Data;
using JaCoreUI.Services.User;

namespace JaCoreUI.ViewModels.User;

public partial class LoginViewModel : PageViewModel
{
    private readonly UserService _userService;
    
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private bool _isLoading = false;
    [ObservableProperty] private string _errorMessage = string.Empty;
    
    public LoginViewModel(UserService userService) : base(ApplicationPageNames.Login, ApplicationPageNames.Login)
    {
        _userService = userService;
    }
    
    [RelayCommand]
    private async Task Login()
    {
        if (!Validate()) return;
        
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            var result = await _userService.LoginAsync(Username, Password);
            if (!result)
            {
                ErrorMessage = "Invalid username or password";
            }
            // Navigation would happen here or be triggered by an event
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login failed: {ex.Message}";
            Console.WriteLine($"Login error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    protected override void OnDesignTimeConstructor()
    {
        // For design-time only
        Username = "admin";
        Password = "password";
    }

    public override bool Validate()
    {
        if (string.IsNullOrEmpty(Username))
        {
            ErrorMessage = "Username is required";
            return false;
        }
        
        if (string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Password is required";
            return false;
        }
        
        return true;
    }
}