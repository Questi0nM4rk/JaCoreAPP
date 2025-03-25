using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Models.Core;
using JaCoreUI.Models.UI.Base;
using System.Collections.ObjectModel;
using System.Linq;

namespace JaCoreUI.Models.Elements
{
    /// <summary>
    /// Represents a basic operation within a production step
    /// </summary>
    public partial class Operation : ProductionElement, IOperationElement
    {
        /// <summary>
        /// UI elements for this operation
        /// </summary>
        [ObservableProperty]
        public partial ObservableCollection<UIElement> UIElements { get; set; } = new();
        
        /// <summary>
        /// Position in the sequence of operations
        /// </summary>
        [ObservableProperty]
        public partial int OrderIndex { get; set; }
        
        /// <summary>
        /// Validates all UI elements in this operation
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