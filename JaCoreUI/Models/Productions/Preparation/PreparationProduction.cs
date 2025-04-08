using System;
using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Models.Core;
using JaCoreUI.Models.Elements.Parameters;
using JaCoreUI.Models.Productions.Base;
using JaCoreUI.Models.Productions.Work;
using System.Collections.ObjectModel;

namespace JaCoreUI.Models.Productions.Preparation;

/// <summary>
/// Preparation production - customizes parameters for a work production
/// </summary>
public partial class PreparationProduction : Production
{
    /// <summary>
    /// Parameters that can be customized for the work production
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<ProductionParameter> Parameters { get; set; } = new();

    public PreparationProduction()
    {
        ProductionTypeValue = ProductionType.Preparation;
    }

    /// <summary>
    /// Creates a work production based on this preparation
    /// </summary>
    public WorkProduction CreateWorkProduction()
    {
        if (string.IsNullOrEmpty(Name))
            throw new ArgumentNullException(nameof(Name));
        
        var work = new WorkProduction
        {
            Name = Name.Replace("- Preparation", "- Work"),
            Description = Description,
            TemplateId = TemplateId,
            CreatedBy = CreatedBy
        };

        // Copy parameters
        foreach (var param in Parameters)
            work.Parameters.Add(new ProductionParameter
            {
                Name = param.Name,
                Value = param.Value,
                ParameterType = param.ParameterType
            });

        // Clone structure with parameters applied
        ApplyParametersAndClone(work);

        return work;
    }

    /// <summary>
    /// Applies parameters and clones the structure to the work production
    /// </summary>
    private void ApplyParametersAndClone(WorkProduction target)
    {
        // Implementation details would depend on how parameters affect
        // specific UI elements and operations
    }
}