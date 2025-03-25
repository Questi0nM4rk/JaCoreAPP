using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Models.Core;
using JaCoreUI.Models.UI.Base;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace JaCoreUI.Models.Elements.Device
{
    /// <summary>
    /// Represents an operation that can be performed by a device
    /// </summary>
    public partial class DeviceOperation : ProductionElement
    {
        /// <summary>
        /// UI elements for this device operation
        /// </summary>
        [ObservableProperty]
        public partial ObservableCollection<UIElement> UIElements { get; set; } = new();
        
        /// <summary>
        /// Position in the sequence of device operations
        /// </summary>
        [ObservableProperty]
        public partial int OrderIndex { get; set; }
        
        /// <summary>
        /// ID of the device this operation belongs to
        /// </summary>
        [ObservableProperty]
        public partial Guid DeviceId { get; set; }
        
        /// <summary>
        /// Whether this is a custom operation or predefined for the device
        /// </summary>
        [ObservableProperty]
        public partial bool IsCustomOperation { get; set; }
        
        /// <summary>
        /// Validates all UI elements in this device operation
        /// </summary>
        public bool ValidateOperation() => UIElements.All(e => e.Validate());
        
        /// <summary>
        /// Updates completion status based on UI element validation
        /// </summary>
        public void UpdateCompletionStatus()
        {
            IsCompleted = ValidateOperation();
        }
    }
}