using JaCoreUI.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JaCoreUI.ViewModels;

public abstract partial class PageViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ApplicationPageNames PageName { get; set; }

    internal PageViewModel(ApplicationPageNames pageName)
    {
        PageName = pageName;
        
        // Detect design time
        if (Avalonia.Controls.Design.IsDesignMode)
            OnDesignTimeConstructor();
    }

    protected abstract void OnDesignTimeConstructor();
}