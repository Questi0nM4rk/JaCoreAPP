using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JaCoreUI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    public partial object? CurrentView { get; set; }

    public ThemeService ThemeService { get; }

    public MainWindowViewModel(ThemeService themeService)
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