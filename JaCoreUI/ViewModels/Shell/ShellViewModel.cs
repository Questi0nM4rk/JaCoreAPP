using System;
using System.Collections.ObjectModel;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Controls;
using JaCoreUI.Data;
using JaCoreUI.Factories;
using JaCoreUI.Models.UI;
using JaCoreUI.Models.User;
using JaCoreUI.Services;
using JaCoreUI.Services.User;
using JaCoreUI.ViewModels.Admin;
using JaCoreUI.ViewModels.Settings;
using Microsoft.Extensions.DependencyInjection;
using CurrentPageService = JaCoreUI.Services.Navigation.CurrentPageService;
using ThemeService = JaCoreUI.Services.Theme.ThemeService;

namespace JaCoreUI.ViewModels.Shell;

public partial class ShellViewModel : ObservableObject
{
    private readonly UserService _userService;
    
    [ObservableProperty] private ThemeService _theme;
    [ObservableProperty] private CurrentPageService _currentPageService;
    [ObservableProperty] private ListItem? _selectedPage;
    [ObservableProperty] private JaCoreUI.Models.User.User? _currentUser;
    [ObservableProperty] private bool _isAuthenticated;
    
    [ObservableProperty]
    private ObservableCollection<ListItem> _sideBarItems = new ObservableCollection<ListItem>
    {
        new("Dashboard", "\ue2f4", ApplicationPageNames.Dashboard),
        new("Zařízení", "\ueba4", ApplicationPageNames.Devices),
        new("Produkce", "\ue0f4", ApplicationPageNames.Productions),
        new("Šablony prdukcí", "\ue0e4", ApplicationPageNames.Templates)
    };

    partial void OnSelectedPageChanged(ListItem? value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        Dispatcher.UIThread.Post(async void () =>
        {
            try
            {
                await CurrentPageService.NavigateTo(value.ParentPage);
            }
            catch (Exception e)
            {
                await ErrorDialog.ShowWithButtonsAsync(message: $"Něco se nepovedlo: {e.Message}",
                                                       title:"Error");
            }
        });
    }

    public ShellViewModel(ThemeService themeService, CurrentPageService currentPageService, UserService userService)
    {
        _theme = themeService;
        _currentPageService = currentPageService;
        _userService = userService;
        
        SelectedPage = SideBarItems[0];
        
        // Subscribe to user changes
        _userService.UserChanged += OnUserChanged;
        
        // Initial check for authentication
        CheckAuthentication();
    }
    
    private async void CheckAuthentication()
    {
        IsAuthenticated = _userService.IsAuthenticated;
        if (IsAuthenticated)
        {
            CurrentUser = await _userService.GetCurrentUserAsync();
            
            // Add admin item if user has admin role
            if (_userService.HasRole("Admin") && !HasAdminPageInSidebar())
            {
                Dispatcher.UIThread.Post(() =>
                {
                    SideBarItems.Add(new("Admin", "\ue8f9", ApplicationPageNames.Admin));
                });
            }
        }
        else
        {
            CurrentUser = null;
            
            // Remove admin item if it exists
            RemoveAdminPageFromSidebar();
            
            // Navigate to login if not authenticated
            await CurrentPageService.NavigateTo(ApplicationPageNames.Login);
        }
    }
    
    private bool HasAdminPageInSidebar()
    {
        foreach (var item in SideBarItems)
        {
            if (item.ParentPage == ApplicationPageNames.Admin)
                return true;
        }
        return false;
    }
    
    private void RemoveAdminPageFromSidebar()
    {
        for (int i = SideBarItems.Count - 1; i >= 0; i--)
        {
            if (SideBarItems[i].ParentPage == ApplicationPageNames.Admin)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    SideBarItems.RemoveAt(i);
                });
                break;
            }
        }
    }
    
    private void OnUserChanged(object? sender, JaCoreUI.Models.User.User? user)
    {
        CheckAuthentication();
    }

    [RelayCommand]
    public async Task GoBack()
    {
        await CurrentPageService.GoBack();

        foreach (var item in SideBarItems)
        {
            if (CurrentPageService.CurrentPage!.SideBarSelectedPage != item.ParentPage)
                continue;

            SelectedPage = item;
            break;
        }
    }
    
    [RelayCommand]
    public async Task Logout()
    {
        await _userService.LogoutAsync();
        // Navigation to login page is handled in CheckAuthentication
    }
}