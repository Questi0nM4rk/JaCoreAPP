using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Models.Production;
using System.Reflection;

namespace JaCoreUI.ViewModels.Production;

/// <summary>
/// ViewModel wrapper for a DynamicUiElementModel, providing presentation logic.
/// </summary>
public partial class DynamicUiElementViewModel : ViewModelBase
{
    private readonly InstructionViewModel _parentInstruction;

    [ObservableProperty]
    private DynamicUiElementModel _model = new();

    // --- Properties derived from Model for easy binding ---
    public DynamicUiElementType ElementType => Model.ElementType;
    public string Label => Model.Label;
    public string? Unit => Model.Unit;
    public double? MinValue => Model.MinValue;
    public double? MaxValue => Model.MaxValue;
    public string? BaseValue => Model.BaseValue;
    public string? BindingTargetProperty => Model.BindingTargetProperty;

    // --- Mode-dependent properties ---

    [ObservableProperty]
    private bool _isWorkMode = false; // Set by the main view model

    [ObservableProperty]
    private bool _isPrepareMode = false; // Set by the main view model

    [ObservableProperty]
    private bool _isTemplateMode = false; // Set by the main view model

    // Value displayed/entered in the UI
    // This needs careful handling based on the mode and binding target.
    // Using reflection here for simplicity to bind to parent properties based on BindingTargetProperty.
    // A more robust solution might use compiled bindings, expressions, or a dedicated mapping service.
    public object? ActualValue
    {
        get
        {
            if (string.IsNullOrEmpty(BindingTargetProperty) || !IsWorkMode) return null;
            try
            {
                PropertyInfo? prop = _parentInstruction.GetType().GetProperty(BindingTargetProperty);
                return prop?.GetValue(_parentInstruction);
            }
            catch { return null; } // Handle reflection errors gracefully
        }
        set
        {
            if (string.IsNullOrEmpty(BindingTargetProperty) || !IsWorkMode) return;
            try
            {
                PropertyInfo? prop = _parentInstruction.GetType().GetProperty(BindingTargetProperty);
                if (prop != null && prop.CanWrite)
                {
                    // Attempt type conversion if necessary (e.g., string from TextBox to double?)
                    object? convertedValue = value;
                    if (prop.PropertyType != value?.GetType())
                    {
                        try 
                        {
                             convertedValue = Convert.ChangeType(value, prop.PropertyType);
                        }
                        catch { /* Conversion failed, potentially set validation error */ return; }
                    }
                    prop.SetValue(_parentInstruction, convertedValue);
                    OnPropertyChanged(); // Notify that ActualValue potentially changed
                }
            }
            catch { /* Handle reflection errors */ }
        }
    }

    // Value calculated based on Template BaseValue and Production TargetAmount (Prepare/Work modes)
    [ObservableProperty]
    private string? _calculatedValue; // Displayed in TextReadOnly or alongside inputs

    // Constructor
    public DynamicUiElementViewModel(InstructionViewModel parent)
    {
        _parentInstruction = parent;
        // React to parent property changes if needed to update ActualValue display
        _parentInstruction.PropertyChanged += (s, e) => 
        {
            if (e.PropertyName == BindingTargetProperty)
            {
                OnPropertyChanged(nameof(ActualValue));
            }
        };
    }

    // Parameterless constructor for designer
    public DynamicUiElementViewModel() : this(new InstructionViewModel()) { }
} 