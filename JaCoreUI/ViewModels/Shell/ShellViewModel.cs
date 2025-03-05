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
    private readonly PageFactory _pageFactory;
    
    [ObservableProperty]
    public partial ThemeService Theme { get; set; }

    [ObservableProperty]
    public partial PageViewModel CurrentPage { get; set; }

    public ShellViewModel(PageFactory pageFactory, ThemeService themeService)
    {
        Theme = themeService;
        _pageFactory = pageFactory;
        CurrentPage = _pageFactory.GetPageViewModel(ApplicationPageNames.Dashboard);
    }

    [RelayCommand]
    public void Home()
    {
        CurrentPage = _pageFactory.GetPageViewModel(ApplicationPageNames.Dashboard);
    }
}