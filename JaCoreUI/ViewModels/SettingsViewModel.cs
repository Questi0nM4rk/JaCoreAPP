using CommunityToolkit.Mvvm.ComponentModel;

namespace JaCoreUI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Title { get; set; } = "Settings";
}