using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Data;
using JaCoreUI.Factories;
using JaCoreUI.Services;
using JaCoreUI.ViewModels.Admin;
using JaCoreUI.ViewModels.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace JaCoreUI.ViewModels.Shell;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private partial NavigationService Navigation { get; set; }
    
    [ObservableProperty]
    public partial ThemeService Theme { get; set; }

    [ObservableProperty]
    public partial PageViewModel? CurrentView { get; set; }

    public ShellViewModel(NavigationService navigationService, ThemeService themeService)
    {
        Navigation = navigationService;
        Theme = themeService;
        Navigation.NavigateTo(ApplicationPageNames.Dashboard);
    }

    [RelayCommand]
    public void Home()
    {
        Navigation.NavigateTo(ApplicationPageNames.Dashboard);
    }
}