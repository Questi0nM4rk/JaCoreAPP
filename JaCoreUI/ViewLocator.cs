using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using JaCoreUI.ViewModels;

namespace JaCoreUI;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var viewModelType = data.GetType();
        var viewName = viewModelType.FullName!
            .Replace("ViewModels", "Views")
            .Replace("ViewModel", "View");
        var viewType = Type.GetType(viewName);
        if (viewType == null) return new TextBlock { Text = $"View Not Found: {viewName}" };

        var control = (Control)Activator.CreateInstance(viewType)!;
        control.DataContext = data;
        return control;
    }

    public bool Match(object? data)
    {
        return data is PageViewModel;
    }
}