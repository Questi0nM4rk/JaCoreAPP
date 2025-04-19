using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Models.Core;
using JaCore.Common.Device;

namespace JaCoreUI.Models.Device;

/// <summary>
/// Represents a physical device that can perform operations
/// </summary>
public partial class Device : ObservableObject, IOperationElement
{
    /// <summary>
    /// Unique identifier for the element
    /// </summary>
    [ObservableProperty]
    public partial int Id { get; set; }

    /// <summary>
    /// Display name of the element
    /// </summary>
    [ObservableProperty]
    public partial string? Name { get; set; }

    /// <summary>
    /// Optional description or notes
    /// </summary>
    [ObservableProperty]
    public partial string? Description { get; set; }
    
    [ObservableProperty]
    public partial DeviceDataState DataState { get; set; }
    
    [ObservableProperty]
    public partial DeviceOperationalState OperationalState { get; set; }
    
    [ObservableProperty]
    public partial DateTimeOffset? CreatedAt { get; set; }
    
    [ObservableProperty]
    public partial DateTimeOffset? ModifiedAt { get; set; }

    /// <summary>
    /// Operations specific to this device
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<int>? DeviceOperationsIds { get; set; }

    /// <summary>
    /// Type of device
    /// </summary>
    [ObservableProperty]
    public partial Category? Category { get; set; }
    
    [ObservableProperty]
    public partial int DeviceCardId { get; set; }

    [ObservableProperty] public partial DeviceCard? DeviceCard { get; set; }

    /// <summary>
    /// Additional device properties
    /// </summary>
    [ObservableProperty]
    public partial Dictionary<string, string>? Properties { get; set; }

    /// <summary>
    /// Position in the sequence of operations
    /// </summary>
    [ObservableProperty]
    public partial int OrderIndex { get; set; }

    public bool HasCard => DeviceCard != null;
    
    public bool HasOperations => DeviceOperationsIds != null && DeviceOperationsIds.Count > 0;

    public bool IsCompleted { get; set; }
    
    public Device()
    {
        // Default constructor
    }
    
    public Device(Device source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        
        Id = source.Id;
        Name = source.Name;
        Description = source.Description;
        DataState = source.DataState;
        OperationalState = source.OperationalState;
        CreatedAt = source.CreatedAt;
        ModifiedAt = source.ModifiedAt;
        Category = source.Category;
        OrderIndex = source.OrderIndex;
        IsCompleted = source.IsCompleted;
        
        // For complex objects, create new instances
        if (source.DeviceCard != null)
        {
            DeviceCard = new DeviceCard(source.DeviceCard);
            DeviceCardId = source.DeviceCardId;
        }

        // Initialize collections to empty if null
        DeviceOperationsIds = source.DeviceOperationsIds != null 
            ? new ObservableCollection<int>(source.DeviceOperationsIds) 
            : new ObservableCollection<int>();
        
        // Copy Properties dictionary
        Properties = source.Properties != null 
            ? new Dictionary<string, string>(source.Properties) 
            : new Dictionary<string, string>();
    }
    
    // Clone method that uses the copy constructor
    public Device Clone()
    {
        return new Device(this);
    }
}