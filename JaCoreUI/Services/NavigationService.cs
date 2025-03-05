using System;
using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Data;
using JaCoreUI.Factories;
using JaCoreUI.ViewModels.Shell;

namespace JaCoreUI.Services;

public class NavigationService(ShellViewModel shell, PageFactory pageFactory) : ObservableObject
{
    public void NavigateTo(ApplicationPageNames pageName)
    {
        shell.CurrentView = pageFactory.GetPageViewModel(pageName);
    }
}