using JaCoreUI.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JaCoreUI.ViewModels;

public partial class PageViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ApplicationPageNames PageName { get; set; }

    protected PageViewModel(ApplicationPageNames pageName)
    {
        PageName = pageName;
        
        // Detect design time
        if (Avalonia.Controls.Design.IsDesignMode)
            OnDesignTimeConstructor();
    }

    protected virtual void OnDesignTimeConstructor() { }
}