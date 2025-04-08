using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Data;
using JaCoreUI.Factories;
using JaCoreUI.Services.User;
using JaCoreUI.ViewModels;

namespace JaCoreUI.Services.Navigation;

public partial class CurrentPageService(PageFactory pageFactory, UserService userService) : ObservableObject
{
    private readonly UserService _userService = userService;
    
    [ObservableProperty] public partial ObservableCollection<PageViewModel> NavigationHistory { get; set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    public partial PageViewModel? CurrentPage { get; private set; }

    public bool CanGoBack => NavigationHistory.Count > 1;

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    public void GoBack()
    {
        if (!CanGoBack) return;

        if (CurrentPage is null)
            throw new NullReferenceException("CurrentPage is null");

        CurrentPage.Validate();

        NavigationHistory.RemoveAt(NavigationHistory.Count - 1);
        CurrentPage = NavigationHistory.LastOrDefault(defaultValue: pageFactory.GetPageViewModel(ApplicationPageNames.Dashboard));
    }

    [RelayCommand]
    public void NavigateTo(ApplicationPageNames name)
    {
        if (CurrentPage is null)
            CurrentPage = pageFactory.GetPageViewModel(ApplicationPageNames.Dashboard);
        
        CurrentPage.Validate();

        var page = pageFactory.GetPageViewModel(name);
        NavigationHistory.Add(page);
        CurrentPage = page;
    }

    [RelayCommand]
    public void ClearHistoryExceptCurrent()
    {
        var currentPage = CurrentPage;
        NavigationHistory.Clear();
        if (currentPage is null)
            throw new NullReferenceException("currentPage is null - ClearHistoryExceptCurrent");
        NavigationHistory.Add(currentPage);
        CurrentPage = currentPage;
    }

    [RelayCommand]
    public void Reset()
    {
        NavigationHistory.Clear();
        // navigate to login 
    }
}