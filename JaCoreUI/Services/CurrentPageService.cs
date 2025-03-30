using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.ViewModels;

namespace JaCoreUI.Services;

public partial class CurrentPageService : ObservableObject
{
    [ObservableProperty]
    public partial PageViewModel CurrentPage { get; set; }
}