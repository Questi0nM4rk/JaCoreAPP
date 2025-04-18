﻿using JaCoreUI.Models.Core;
using JaCoreUI.Models.Productions.Base;
using JaCoreUI.Models.Productions.Preparation;
using System;
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
        CloneStructure(prep);

        return prep;
    }

    /// <summary>
    /// Clones the structure of this template to the target production
    /// </summary>
    private void CloneStructure(Production target)
    {
        foreach (var step in Steps)
        {
            var clonedStep = new Elements.Step
            {
                Name = step.Name,
                Description = step.Description,
                OrderIndex = step.OrderIndex
            };

            foreach (var operation in step.Operations)
                // Clone operations based on their type
                if (operation is Elements.Operation op)
                {
                    var clonedOp = new Elements.Operation
                    {
                        Name = op.Name,
                        Description = op.Description,
                        OrderIndex = op.OrderIndex
                    };

                    // Clone UI elements
                    foreach (var uiElement in op.UIElements)
                    {
                        // Create a new UI element of the same type
                        var clonedElement = uiElement.Clone();
                        clonedOp.UIElements.Add(clonedElement);
                    }

                    clonedStep.Operations.Add(clonedOp);
                }
                else if (operation is Device.Device device)
                {
                    var clonedDevice = new Device.Device
                    {
                        Name = device.Name,
                        Description = device.Description,
                        OrderIndex = device.OrderIndex,
                        Category = device.Category,
                        DeviceCard = device.DeviceCard
                    };

                    // Clone device operations
                    foreach (var devOp in device.DeviceOperations)
                    {
                        var clonedDevOp = new DeviceOperation
                        {
                            Name = devOp.Name,
                            Description = devOp.Description,
                            OrderIndex = devOp.OrderIndex,
                            DeviceId = clonedDevice.Id
                        };

                        // Clone UI elements
                        foreach (var uiElement in devOp.UiElements)
                        {
                            var clonedElement = uiElement.Clone();
                            clonedDevOp.UiElements.Add(clonedElement);
                        }

                        clonedDevice.DeviceOperations.Add(clonedDevOp);
                    }

                    clonedStep.Operations.Add(clonedDevice);
                }

            target.Steps.Add(clonedStep);
        }
    }
}