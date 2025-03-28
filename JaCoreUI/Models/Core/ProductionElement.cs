﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace JaCoreUI.Models.Core
{
    /// <summary>
    /// Base class for all production hierarchy elements
    /// </summary>
    public abstract partial class ProductionElement : ObservableObject
    {
        /// <summary>
        /// Unique identifier for the element
        /// </summary>
        [ObservableProperty]
        public partial Guid Id { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// Display name of the element
        /// </summary>
        [ObservableProperty]
        public partial string Name { get; set; }
        
        /// <summary>
        /// Optional description or notes
        /// </summary>
        [ObservableProperty]
        public partial string Description { get; set; }
        
        /// <summary>
        /// Indicates whether this element has been completed
        /// </summary>
        [ObservableProperty]
        public partial bool IsCompleted { get; set; }
        
        /// <summary>
        /// Timestamp when the element was created
        /// </summary>
        [ObservableProperty]
        public partial DateTime CreatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Timestamp when the element was last modified
        /// </summary>
        [ObservableProperty]
        public partial DateTime ModifiedAt { get; set; } = DateTime.Now;
    }
}