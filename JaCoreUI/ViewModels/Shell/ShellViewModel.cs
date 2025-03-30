using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Controls;
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
    public partial CurrentPageService CurrentPageService { get; set; }

    [ObservableProperty]
    public partial object? SelectedPage { get; set; }

    partial void OnSelectedPageChanged(object? value)
    {
        if (value is not ListItem listItem)
            throw new ArgumentException();

        CurrentPageService.CurrentPage = (string)listItem.Content! switch
        {
            "Dashboard" => _pageFactory.GetPageViewModel(ApplicationPageNames.Dashboard),
            "Zařízení" => _pageFactory.GetPageViewModel(ApplicationPageNames.Devices),
            _ => CurrentPageService.CurrentPage
        };
    }

    public ShellViewModel(PageFactory pageFactory, ThemeService themeService, CurrentPageService currentPageService)
    {
        Theme = themeService;
        _pageFactory = pageFactory;
        CurrentPageService = currentPageService;
        CurrentPageService.CurrentPage = _pageFactory.GetPageViewModel(ApplicationPageNames.Devices);
    }
}