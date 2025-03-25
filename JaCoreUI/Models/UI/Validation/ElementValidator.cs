using JaCoreUI.Models.UI.Base;
using System.Collections.Generic;
using System.Linq;

namespace JaCoreUI.Models.UI.Validation
{
    /// <summary>
    /// Utility class for validating collections of UI elements
    /// </summary>
    public static class ElementValidator
    {
        /// <summary>
        /// Validates a collection of UI elements
        /// </summary>
        /// <param name="elements">Elements to validate</param>
        /// <returns>Validation result with details</returns>
        public static ValidationResult ValidateElements(IEnumerable<UIElement> elements)
        {
            var result = new ValidationResult();
            
            foreach (var element in elements)
            {
                if (!element.Validate())
                {
                    result.IsValid = false;
                    result.InvalidElements.Add(element);
                    result.ValidationMessages.Add($"'{element.Label}' has an invalid value.");
                }
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// Results from an element validation operation
    /// </summary>
    public partial class ValidationResult
    {
        /// <summary>
        /// Whether the overall validation passed
        /// </summary>
        public bool IsValid { get; set; } = true;
        
        /// <summary>
        /// List of elements that failed validation
        /// </summary>
        public List<UIElement> InvalidElements { get; } = new();
        
        /// <summary>
        /// Validation error messages
        /// </summary>
        public List<string> ValidationMessages { get; } = new();
    }
}