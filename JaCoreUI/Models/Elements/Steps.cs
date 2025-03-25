using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Models.Core;
using System.Collections.ObjectModel;
using System.Linq;

namespace JaCoreUI.Models.Elements
{
    /// <summary>
    /// Represents a step in a production process
    /// </summary>
    public partial class Step : ProductionElement
    {
        /// <summary>
        /// Operations contained within this step
        /// </summary>
        [ObservableProperty]
        public partial ObservableCollection<IOperationElement> Operations { get; set; } = new();
        
        /// <summary>
        /// Position in the sequence of steps
        /// </summary>
        [ObservableProperty]
        public partial int OrderIndex { get; set; }
        
        /// <summary>
        /// Validates that all operations in this step are completed
        /// </summary>
        public bool ValidateStepCompletion() => Operations.All(o => o.IsCompleted);
        
        /// <summary>
        /// Checks if all operations are completed and updates the step completion status
        /// </summary>
        public void UpdateCompletionStatus()
        {
            IsCompleted = ValidateStepCompletion();
        }
    }
}