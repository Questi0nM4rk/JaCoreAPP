using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Models.Core;

namespace JaCoreUI.Models.UI.Base
{
    /// <summary>
    /// Base class for UI elements that can be used in operations
    /// </summary>
    public abstract partial class UIElement : ProductionElement
    {
        /// <summary>
        /// Whether input is required for this element
        /// </summary>
        [ObservableProperty]
        public partial bool IsRequired { get; set; }
        
        /// <summary>
        /// Whether this element should be read-only
        /// </summary>
        [ObservableProperty]
        public partial bool IsReadOnly { get; set; }
        
        /// <summary>
        /// Whether current value is valid
        /// </summary>
        [ObservableProperty]
        public partial bool IsValid { get; set; }
        
        /// <summary>
        /// Display label for the element
        /// </summary>
        [ObservableProperty]
        public partial string Label { get; set; }
        
        /// <summary>
        /// Help text to display
        /// </summary>
        [ObservableProperty]
        public partial string HelpText { get; set; }
        
        /// <summary>
        /// Validates the current state of the UI element
        /// </summary>
        public abstract bool Validate();
        
        /// <summary>
        /// Creates a deep clone of this UI element
        /// </summary>
        public abstract UIElement Clone();
    }
}