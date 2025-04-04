using JaCoreUI.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Controls;

namespace JaCoreUI.ViewModels;

public abstract partial class PageViewModel : ViewModelBase
{
    [ObservableProperty]
    public required partial ApplicationPageNames PageName { get; set; }

    [ObservableProperty]
    public required partial ApplicationPageNames SideBarSelectedPage { get; set; }
    
    internal PageViewModel(ApplicationPageNames pageName, ApplicationPageNames sideBarSelectedPage)
    {
        PageName = pageName;
        SideBarSelectedPage = sideBarSelectedPage;
        
        // Detect design time
        if (Avalonia.Controls.Design.IsDesignMode)
            OnDesignTimeConstructor();
    }

    protected abstract void OnDesignTimeConstructor();
}