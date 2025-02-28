using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Services;

namespace JaCoreUI.ViewModels;

public partial class DashboardViewModel(ThemeService themeService) : ObservableObject
{
    public ThemeService ThemeService { get; } = themeService;

    [ObservableProperty]
    public partial string Title { get; set; } = "Dashboard";
}