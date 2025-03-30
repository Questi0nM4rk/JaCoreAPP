using System;

namespace JaCoreUI.Models.Core
{
    /// <summary>
    /// Interface for elements that can be operations within a Step
    /// </summary>
    public interface IOperationElement
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        int Id { get; }
        
        /// <summary>
        /// Display name
        /// </summary>
        string Name { get; set; }
        
        /// <summary>
        /// Completion status
        /// </summary>
        bool IsCompleted { get; set; }
        
        /// <summary>
        /// Position within the parent collection
        /// </summary>
        int OrderIndex { get; set; }
    }
}