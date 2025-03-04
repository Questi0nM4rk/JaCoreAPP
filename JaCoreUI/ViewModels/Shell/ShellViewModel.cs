using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Services;
using JaCoreUI.ViewModels.Admin;
using JaCoreUI.ViewModels.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace JaCoreUI.ViewModels.Shell;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    public partial object? CurrentView { get; set; }

    public ThemeService ThemeService { get; }

    public ShellViewModel(ThemeService themeService)
    {
        ThemeService = themeService;
        Navigate("Dashboard");
    }
    
    [RelayCommand]
    private void Navigate(string? destination)
    {
        CurrentView = destination switch
        {
            "Dashboard" => App.Current?.Services?.GetRequiredService<DashboardViewModel>(),
            "Settings" => App.Current?.Services?.GetRequiredService<SettingsViewModel>(),
            _ => CurrentView
        };
    }
}