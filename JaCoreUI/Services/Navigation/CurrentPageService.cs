using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Data;
using JaCoreUI.Factories;
using JaCoreUI.Models.UI;
using JaCoreUI.Services.User;
using JaCoreUI.ViewModels;
using MsBox.Avalonia.Enums;

namespace JaCoreUI.Services.Navigation;

public partial class CurrentPageService : ObservableObject
{
    private readonly PageFactory _pageFactory;
    
    [ObservableProperty] public partial ObservableCollection<PageViewModel> NavigationHistory { get; set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    public partial PageViewModel? CurrentPage { get; private set; }

    public bool CanGoBack => NavigationHistory.Count > 1;

    public CurrentPageService(PageFactory pageFactory)
    {
        _pageFactory = pageFactory;
    }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    public async Task GoBack()
    {
        if (!CanGoBack) return;

        if (CurrentPage is null)
            throw new NullReferenceException("CurrentPage is null");

        if (!CurrentPage.Validate())
        {
            var result = await ErrorDialog.ShowWithButtonsAsync(
                "You have unsaved changes. Discard changes and continue?", 
                "Unsaved Changes",
                ButtonEnum.YesNo);
        
            if (result == ButtonResult.No)
            {
                return;
            }
        }

        NavigationHistory.RemoveAt(NavigationHistory.Count - 1);
        CurrentPage = NavigationHistory.LastOrDefault(defaultValue: _pageFactory.GetPageViewModel(ApplicationPageNames.Dashboard));
    }

    [RelayCommand]
    public async Task NavigateTo(ApplicationPageNames name)
    {
        CurrentPage ??= _pageFactory.GetPageViewModel(ApplicationPageNames.Dashboard);
        
        if (!CurrentPage.Validate())
        {
            var result = await ErrorDialog.ShowWithButtonsAsync(
                "You have unsaved changes. Discard changes and continue?", 
                "Unsaved Changes",
                ButtonEnum.YesNo);
        
            if (result == ButtonResult.No)
            {
                return;
            }
        }

        var page = _pageFactory.GetPageViewModel(name);
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