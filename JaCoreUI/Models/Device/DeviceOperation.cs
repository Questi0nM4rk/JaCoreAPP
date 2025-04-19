using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Models.Core;
using JaCoreUI.Models.UI.Base;

namespace JaCoreUI.Models.Device;

/// <summary>
/// Represents an operation that can be performed by a device
/// </summary>
public partial class DeviceOperation : ProductionElement
{

    /// <summary>
    /// UI elements for this device operation
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<int> UiElementsIDs { get; set; } = [];
    
    /// <summary>
    /// UI elements for this device operation
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<UIElement> UiElements { get; set; } = [];

    /// <summary>
    /// Position in the sequence of device operations
    /// </summary>
    [ObservableProperty]
    public partial int OrderIndex { get; set; }

    /// <summary>
    /// ID of the device this operation belongs to
    /// </summary>
    [ObservableProperty]
    public partial int DeviceId { get; set; }

    /// <summary>
    /// Whether this Operation is required when using the device
    /// </summary>
    [ObservableProperty]
    public partial bool IsRequired { get; set; }

    public DeviceOperation()
    {
        // Default constructor
    }
    
    public DeviceOperation(DeviceOperation source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        
        // Copy basic properties from ProductionElement
        Id = source.Id;
        Name = source.Name ?? string.Empty;
        Description = source.Description;
        IsCompleted = source.IsCompleted;
        
        // Copy DeviceOperation specific properties
        OrderIndex = source.OrderIndex;
        DeviceId = source.DeviceId;
        IsRequired = source.IsRequired;
        
        // Copy collections
        UiElementsIDs = source.UiElementsIDs != null 
            ? new ObservableCollection<int>(source.UiElementsIDs) 
            : new ObservableCollection<int>();
        
        // Deep copy UI elements if they exist
        if (source.UiElements != null)
        {
            UiElements = new ObservableCollection<UIElement>(
                source.UiElements.Select(e => e.Clone()));
        }
        else
        {
            UiElements = new ObservableCollection<UIElement>();
        }
    }

    /// <summary>
    /// Validates all UI elements in this device operation
    /// </summary>
    public bool ValidateOperation()
    {
        if (UiElements == null || UiElements.Count == 0)
            return true; // No UI elements to validate, consider it valid
            
        return UiElements.All(e => e != null && e.Validate());
    }

    /// <summary>
    /// Updates completion status based on UI element validation
    /// </summary>
    public void UpdateCompletionStatus()
    {
        IsCompleted = ValidateOperation();
    }
}