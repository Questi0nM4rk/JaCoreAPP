namespace JaCoreUI.Models.Production;

/// <summary>
/// Defines the types of UI elements that can be dynamically generated within an instruction.
/// </summary>
public enum DynamicUiElementType
{
    Label,          // Simple text display
    TextReadOnly,   // Read-only text display (e.g., for calculated values in Prepare mode)
    TextInput,      // Standard text input
    NumericInput,   // Input for numbers (potentially with min/max/unit)
    Checkbox        // Simple boolean checkbox 
    // Add other types as needed (e.g., ComboBox, DatePicker)
} 