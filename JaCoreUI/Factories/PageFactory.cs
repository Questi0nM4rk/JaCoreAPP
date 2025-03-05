using JaCoreUI.Data;
using JaCoreUI.ViewModels;
using System;

namespace JaCoreUI.Factories;

public class PageFactory(Func<ApplicationPageNames, PageViewModel> factory)
{
    public PageViewModel GetPageViewModel(ApplicationPageNames pageName) => factory.Invoke(pageName);
}