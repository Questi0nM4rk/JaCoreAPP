using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Models.Core;

namespace JaCoreUI.Models.Device;

/// <summary>
/// Represents a physical device that can perform operations
/// </summary>
public partial class Device : ProductionElement, IOperationElement
{
    /// <summary>
    /// Operations specific to this device
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<DeviceOperation> DeviceOperations { get; set; } = new();

    /// <summary>
    /// Type of device
    /// </summary>
    [ObservableProperty]
    public partial Category? Category { get; set; }

    [ObservableProperty] public partial DeviceCard? DeviceCard { get; set; }

    /// <summary>
    /// Additional device properties
    /// </summary>
    [ObservableProperty]
    public partial Dictionary<string, string> Properties { get; set; } = new();

    /// <summary>
    /// Position in the sequence of operations
    /// </summary>
    [ObservableProperty]
    public partial int OrderIndex { get; set; }

    public bool HasCard => DeviceCard != null;

    /// <summary>
    /// Validates all device operations
    /// </summary>
    public bool ValidateDevice()
    {
        return DeviceOperations.All(o => o.IsCompleted);
    }

    /// <summary>
    /// Updates completion status based on device operation validation
    /// </summary>
    public void UpdateCompletionStatus()
    {
        IsCompleted = ValidateDevice();
    }
}