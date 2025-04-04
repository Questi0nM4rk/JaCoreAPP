﻿using System;
using System.Collections.ObjectModel;
using System.Runtime.ConstrainedExecution;
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
    [ObservableProperty]
    public partial ThemeService Theme { get; set; }

    [ObservableProperty]
    public partial CurrentPageService CurrentPageService { get; set; }

    [ObservableProperty]
    public partial ListItem? SelectedPage { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ListItem> SideBarItems { get; set; } =
    [
        new (content: "Dashboard", iconText: "\ue2f4", ApplicationPageNames.Dashboard),
        new (content: "Zařízení", iconText: "\ueba4", ApplicationPageNames.Devices),
        new (content: "Produkce", iconText: "\ue0f4", ApplicationPageNames.Productions),
        new (content: "Šablony prdukcí", iconText: "\ue0e4", ApplicationPageNames.Templates),
    ];

    partial void OnSelectedPageChanged(ListItem? value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        CurrentPageService.NavigateTo(value.ParentPage);
    }

    public ShellViewModel(ThemeService themeService, CurrentPageService currentPageService)
    {
        Theme = themeService;
        CurrentPageService = currentPageService;
        CurrentPageService.NavigateTo(ApplicationPageNames.Dashboard);
        SelectedPage = SideBarItems[0];
    }

    [RelayCommand]
    public void GoBack()
    {
        CurrentPageService.GoBack();
        
        foreach (var item in SideBarItems)
        {
            if (CurrentPageService.CurrentPage!.SideBarSelectedPage != item.ParentPage)
                continue;
            
            SelectedPage = item;
            break;

        }
    }
}