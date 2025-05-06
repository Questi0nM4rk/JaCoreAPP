using Avalonia.Controls;
using Avalonia.Controls.Templates;
using JaCoreUI.Models.Production;
using JaCoreUI.ViewModels.Production;
using System;

namespace JaCoreUI.DataTemplates; // Changed namespace

public class DynamicUiTemplateSelector : IDataTemplate
{
    // Define properties for actual templates (to be set in XAML)
    // Ensure these names match the keys you use in XAML resources.
    public IDataTemplate? LabelTemplate { get; set; }
    public IDataTemplate? TextReadOnlyTemplate { get; set; }
    public IDataTemplate? TextInputTemplate { get; set; }
    public IDataTemplate? NumericInputTemplate { get; set; }
    public IDataTemplate? CheckboxTemplate { get; set; }
    public IDataTemplate? FallbackTemplate { get; set; }

    public Control? Build(object? param)
    {
        if (param is not DynamicUiElementViewModel viewModel) 
        {
             return FallbackTemplate?.Build(param);
        }

        return viewModel.ElementType switch
        {
            DynamicUiElementType.Label => LabelTemplate?.Build(viewModel),
            DynamicUiElementType.TextReadOnly => TextReadOnlyTemplate?.Build(viewModel),
            DynamicUiElementType.TextInput => TextInputTemplate?.Build(viewModel),
            DynamicUiElementType.NumericInput => NumericInputTemplate?.Build(viewModel),
            DynamicUiElementType.Checkbox => CheckboxTemplate?.Build(viewModel),
            _ => FallbackTemplate?.Build(viewModel) // Default case
        };
    }

    public bool Match(object? data)
    {
        // This selector is specifically for DynamicUiElementViewModel
        return data is DynamicUiElementViewModel;
    }
} 