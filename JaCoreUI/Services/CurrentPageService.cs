using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Data;
using JaCoreUI.Factories;
using JaCoreUI.ViewModels;

namespace JaCoreUI.Services;

public partial class CurrentPageService(PageFactory pageFactory) : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<PageViewModel> NavigationHistory { get; set; } = new();
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    public partial PageViewModel? CurrentPage { get; private set; }
    
    public bool CanGoBack => NavigationHistory.Count > 1;
    
    [RelayCommand(CanExecute = nameof(CanGoBack))]
    public void GoBack()
    {
        if (!CanGoBack) return;
        NavigationHistory.RemoveAt(NavigationHistory.Count - 1);
        CurrentPage = NavigationHistory.LastOrDefault();
    }
    
    [RelayCommand]
    public void NavigateTo(ApplicationPageNames name)
    {
        var page = pageFactory.GetPageViewModel(name);
        NavigationHistory.Add(page);
        CurrentPage = page;
    }
    
    [RelayCommand]
    public void ClearHistoryExceptCurrent()
    {
        if (CurrentPage == null) return;
        
        var currentPage = CurrentPage;
        NavigationHistory.Clear();
        NavigationHistory.Add(currentPage);
        CurrentPage = currentPage;
    }
    
    [RelayCommand]
    public void Reset()
    {
        NavigationHistory.Clear();
        CurrentPage = null;
    }
}