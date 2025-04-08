using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace JaCoreUI.Models.Elements.Parameters;

/// <summary>
/// Represents a parameter that can be customized in a production
/// </summary>
public partial class ProductionParameter : ObservableObject
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    [ObservableProperty]
    public partial Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Name of the parameter
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; }

    /// <summary>
    /// Value of the parameter
    /// </summary>
    [ObservableProperty]
    public partial object Value { get; set; }

    /// <summary>
    /// Type of parameter
    /// </summary>
    [ObservableProperty]
    public partial ParameterType ParameterType { get; set; }

    /// <summary>
    /// Description of the parameter
    /// </summary>
    [ObservableProperty]
    public partial string Description { get; set; }
}

/// <summary>
/// Types of parameters supported in the system
/// </summary>
public enum ParameterType
{
    String,
    Number,
    Boolean,
    DateTime,
    Selection
}