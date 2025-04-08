using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Models.UI.Base;
using System;
using System.Text.RegularExpressions;

namespace JaCoreUI.Models.UI;

/// <summary>
/// Text input UI element
/// </summary>
public partial class TextBoxElement : UIElement
{
    /// <summary>
    /// Current text value
    /// </summary>
    [ObservableProperty]
    public partial string Value { get; set; }

    /// <summary>
    /// Expected value for validation
    /// </summary>
    [ObservableProperty]
    public partial string ExpectedValue { get; set; }

    /// <summary>
    /// Whether validation should be case-sensitive
    /// </summary>
    [ObservableProperty]
    public partial bool CaseSensitive { get; set; }

    /// <summary>
    /// Maximum length of the text
    /// </summary>
    [ObservableProperty]
    public partial int MaxLength { get; set; } = 255;

    /// <summary>
    /// Optional regex pattern for validation
    /// </summary>
    [ObservableProperty]
    public partial string ValidationPattern { get; set; }

    /// <summary>
    /// Validates the current text value
    /// </summary>
    public override bool Validate()
    {
        if (!IsRequired && string.IsNullOrEmpty(Value))
            return true;

        if (IsRequired && string.IsNullOrEmpty(Value))
            return false;

        // Check expected value if specified
        if (!string.IsNullOrEmpty(ExpectedValue))
        {
            var matches = CaseSensitive
                ? Value == ExpectedValue
                : Value?.Equals(ExpectedValue, StringComparison.OrdinalIgnoreCase) == true;

            if (!matches)
                return false;
        }

        // Check regex pattern if specified
        if (!string.IsNullOrEmpty(ValidationPattern) && !string.IsNullOrEmpty(Value) &&
            !Regex.IsMatch(Value, ValidationPattern))
            return false;

        return true;
    }

    /// <summary>
    /// Creates a deep clone of this text box element
    /// </summary>
    public override UIElement Clone()
    {
        return new TextBoxElement
        {
            Name = Name,
            Description = Description,
            IsRequired = IsRequired,
            IsReadOnly = IsReadOnly,
            Label = Label,
            HelpText = HelpText,
            ExpectedValue = ExpectedValue,
            CaseSensitive = CaseSensitive,
            MaxLength = MaxLength,
            ValidationPattern = ValidationPattern
        };
    }
}