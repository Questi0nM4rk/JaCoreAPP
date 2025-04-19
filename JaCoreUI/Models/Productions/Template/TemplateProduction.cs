using JaCoreUI.Models.Core;
using JaCoreUI.Models.Productions.Base;
using JaCoreUI.Models.Productions.Preparation;
using System;
using Force.DeepCloner;
using JaCoreUI.Models.Device;

namespace JaCoreUI.Models.Productions.Template;

/// <summary>
/// Template production - defines reusable production structures
/// </summary>
public partial class TemplateProduction : Production
{
    public TemplateProduction()
    {
        ProductionTypeValue = ProductionType.Template;
    }

    /// <summary>
    /// Creates a preparation production based on this template
    /// </summary>
    public PreparationProduction CreatePreparationProduction()
    {
        var prep = new PreparationProduction
        {
            Name = $"{Name} - Preparation",
            Description = Description,
            TemplateId = Id,
            CreatedBy = CreatedBy
        };

        // Clone the structure
        return prep.DeepClone();
    }
}