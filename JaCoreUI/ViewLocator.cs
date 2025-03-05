using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using JaCoreUI.ViewModels;

namespace JaCoreUI;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null) return null;
        
        var viewModelType = data.GetType();
        var viewName = viewModelType.FullName!
            .Replace("ViewModels", "Views")
            .Replace("ViewModel", "View");
        
        var viewType = Type.GetType(viewName);
        
        if (viewType == null)
        {
            return new TextBlock { Text = $"View Not Found: {viewName}" };
        }

        return (Control)Activator.CreateInstance(viewType)!;
    }

    public bool Match(object? data) => data is PageViewModel;
}
