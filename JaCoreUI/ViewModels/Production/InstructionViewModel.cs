using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using JaCoreUI.Models.Production; // For DynamicUiElementModel

namespace JaCoreUI.ViewModels.Production;

/// <summary>
/// Represents an instruction within a production step.
/// </summary>
public partial class InstructionViewModel : ViewModelBase
{
    private readonly StepViewModel _parentStep; // To notify parent

    [ObservableProperty]
    private string _name = "New Instruction";

    [ObservableProperty]
    private ObservableCollection<DynamicUiElementViewModel> _dynamicUiElements = [];

    // Completion Status (Directly settable in Work mode)
    [ObservableProperty]
    private bool? _isCompleted = false;

    // Placeholder properties for dynamic UI element data binding
    // In a real app, these might be generated or mapped differently.
    [ObservableProperty] private string? _actualText1;
    [ObservableProperty] private string? _actualText2;
    [ObservableProperty] private double? _actualValue1;
    [ObservableProperty] private double? _actualValue2;
    [ObservableProperty] private bool? _actualFlag1;

    // Placeholder for calculated values (populated in Prepare mode)
    [ObservableProperty] private string? _calculatedValueText1;
    [ObservableProperty] private string? _calculatedValueText2;
    [ObservableProperty] private double? _calculatedValueNumeric1;
    [ObservableProperty] private double? _calculatedValueNumeric2;


    // Constructor accepting parent for notification
    public InstructionViewModel(StepViewModel parent)
    {
        _parentStep = parent;
        // DynamicUiElements.CollectionChanged += ... // Might not need this if elements don't affect completion
    }

    // Design-time constructor
    public InstructionViewModel() : this(new StepViewModel()) // Provide a dummy parent for design time
    { 
        // Add sample elements for design view
        if (Avalonia.Controls.Design.IsDesignMode)
        {
            DynamicUiElements.Add(new DynamicUiElementViewModel(this) { Model = new DynamicUiElementModel { ElementType = DynamicUiElementType.Label, Label = "Ingredient A:" } });
            DynamicUiElements.Add(new DynamicUiElementViewModel(this) { Model = new DynamicUiElementModel { ElementType = DynamicUiElementType.NumericInput, Label = "Amount", BindingTargetProperty = nameof(ActualValue1), Unit="kg", BaseValue="10.5"}, CalculatedValue = "12.6" });
            DynamicUiElements.Add(new DynamicUiElementViewModel(this) { Model = new DynamicUiElementModel { ElementType = DynamicUiElementType.TextInput, Label = "Batch ID", BindingTargetProperty = nameof(ActualText1), BaseValue="DefaultID"} });
        }
    }

    partial void OnIsCompletedChanged(bool? value)
    {
        _parentStep.ChildCompletionChanged(); // Notify parent step
    }
}
 