using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Models.UI.Base;
using System;

namespace JaCoreUI.Models.UI;

/// <summary>
/// Numeric input UI element
/// </summary>
public partial class NumberBoxElement : UIElement
{
    /// <summary>
    /// Current numeric value
    /// </summary>
    [ObservableProperty]
    public partial decimal Value { get; set; }

    /// <summary>
    /// Minimum allowed value
    /// </summary>
    [ObservableProperty]
    public partial decimal MinValue { get; set; }

    /// <summary>
    /// Maximum allowed value
    /// </summary>
    [ObservableProperty]
    public partial decimal MaxValue { get; set; } = decimal.MaxValue;

    /// <summary>
    /// Expected value for validation
    /// </summary>
    [ObservableProperty]
    public partial decimal? ExpectedValue { get; set; }

    /// <summary>
    /// Tolerance for value comparison
    /// </summary>
    [ObservableProperty]
    public partial decimal Tolerance { get; set; }

    /// <summary>
    /// Number of decimal places to display
    /// </summary>
    [ObservableProperty]
    public partial int DecimalPlaces { get; set; } = 2;

    /// <summary>
    /// Unit of measure for the value
    /// </summary>
    [ObservableProperty]
    public partial string UnitOfMeasure { get; set; }

    /// <summary>
    /// Validates the current numeric value
    /// </summary>
    public override bool Validate()
    {
        if (!IsRequired && Value == 0)
            return true;

        // Check range
        if (Value < MinValue || Value > MaxValue)
            return false;

        // Check expected value if specified
        if (ExpectedValue.HasValue)
        {
            var withinTolerance = Math.Abs(Value - ExpectedValue.Value) <= Tolerance;
            if (!withinTolerance)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Creates a deep clone of this number box element
    /// </summary>
    public override UIElement Clone()
    {
        return new NumberBoxElement
        {
            Name = Name,
            Description = Description,
            IsRequired = IsRequired,
            IsReadOnly = IsReadOnly,
            Label = Label,
            HelpText = HelpText,
            MinValue = MinValue,
            MaxValue = MaxValue,
            ExpectedValue = ExpectedValue,
            Tolerance = Tolerance,
            DecimalPlaces = DecimalPlaces,
            UnitOfMeasure = UnitOfMeasure
        };
    }
}