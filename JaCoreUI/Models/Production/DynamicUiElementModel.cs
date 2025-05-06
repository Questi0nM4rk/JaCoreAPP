using CommunityToolkit.Mvvm.ComponentModel;

namespace JaCoreUI.Models.Production;

/// <summary>
/// Represents the definition of a UI element to be dynamically generated within an instruction.
/// This model defines the configuration for the UI element.
/// </summary>
public partial class DynamicUiElementModel : ObservableObject // Inherit for potential future property changes if needed
{
    [ObservableProperty]
    private DynamicUiElementType _elementType;

    [ObservableProperty]
    private string _label = string.Empty;

    /// <summary>
    /// The name of the target property on the InstructionViewModel to bind the actual value to (e.g., "ActualValue1", "ActualText1").
    /// Used primarily for input types during Work mode.
    /// </summary>
    [ObservableProperty]
    private string? _bindingTargetProperty; 

    /// <summary>
    /// The unit of measurement (e.g., "kg", "mL", "°C").
    /// </summary>
    [ObservableProperty]
    private string? _unit;

    /// <summary>
    /// The minimum allowed value (for NumericInput).
    /// </summary>
    [ObservableProperty]
    private double? _minValue;

    /// <summary>
    /// The maximum allowed value (for NumericInput).
    /// </summary>
    [ObservableProperty]
    private double? _maxValue;

    /// <summary>
    /// The base value defined in the template (used for scaling in Prepare mode).
    /// Stored as string to accommodate different types (numeric, text, bool).
    /// </summary>
    [ObservableProperty]
    private string? _baseValue;

    // Consider adding other configuration properties like:
    // - IsRequired (bool)
    // - ValidationRules (string/object)
    // - Tooltip (string)
    // - PlaceholderText (string)
    // - OptionsSource (for ComboBox)
    // - VisibilityCondition (string - advanced feature)
} 