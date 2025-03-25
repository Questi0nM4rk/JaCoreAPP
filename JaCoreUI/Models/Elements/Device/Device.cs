using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Models.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JaCoreUI.Models.Elements.Device
{
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
        public partial string DeviceType { get; set; }
        
        /// <summary>
        /// Serial number or unique identifier of physical device
        /// </summary>
        [ObservableProperty]
        public partial string SerialNumber { get; set; }
        
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
        
        /// <summary>
        /// Validates all device operations
        /// </summary>
        public bool ValidateDevice() => DeviceOperations.All(o => o.IsCompleted);
        
        /// <summary>
        /// Updates completion status based on device operation validation
        /// </summary>
        public void UpdateCompletionStatus()
        {
            IsCompleted = ValidateDevice();
        }
    }
}