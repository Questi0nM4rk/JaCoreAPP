using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Models.Core;
using JaCoreUI.Models.Elements;
using System;
using System.Collections.ObjectModel;

namespace JaCoreUI.Models.Productions.Base;

/// <summary>
/// Base class for all production types
/// </summary>
public abstract partial class Production : ProductionElement
{
    /// <summary>
    /// Type of production
    /// </summary>
    [ObservableProperty]
    public partial ProductionType ProductionTypeValue { get; set; }

    /// <summary>
    /// User who created the production
    /// </summary>
    [ObservableProperty]
    public partial string? CreatedBy { get; set; }

    /// <summary>
    /// Optional reference to template production ID
    /// </summary>
    [ObservableProperty]
    public partial int? TemplateId { get; set; }
    
    /// <summary>
    /// Validates the entire production structure
    /// </summary>
    public virtual bool Validate()
    {
        foreach (var step in Steps)
            if (!step.ValidateStepCompletion())
                return false;

        return true;
    }
}