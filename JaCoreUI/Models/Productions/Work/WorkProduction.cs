using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Models.Core;
using JaCoreUI.Models.Elements.Parameters;
using JaCoreUI.Models.Productions.Base;
using System;
using System.Collections.ObjectModel;

namespace JaCoreUI.Models.Productions.Work;

/// <summary>
/// Work production - used for actual production execution by users
/// </summary>
public partial class WorkProduction : Production
{
    /// <summary>
    /// Parameters applied to this work production
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<ProductionParameter> Parameters { get; set; } = new();

    /// <summary>
    /// When work was started
    /// </summary>
    [ObservableProperty]
    public partial DateTime StartDate { get; set; }

    /// <summary>
    /// When work was completed (if it is)
    /// </summary>
    [ObservableProperty]
    public partial DateTime? CompletionDate { get; set; }

    /// <summary>
    /// User assigned to execute this work
    /// </summary>
    [ObservableProperty]
    public partial string AssignedTo { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the work
    /// </summary>
    [ObservableProperty]
    public partial WorkStatus Status { get; set; } = WorkStatus.NotStarted;

    public WorkProduction()
    {
        ProductionTypeValue = ProductionType.Work;
    }

    /// <summary>
    /// Marks the work as complete and records the completion time
    /// </summary>
    public void Complete()
    {
        if (CompletionDate.HasValue)
            return;

        CompletionDate = DateTime.Now;
        Status = WorkStatus.Completed;
        IsCompleted = true;
    }
}

/// <summary>
/// Possible statuses for a work production
/// </summary>
public enum WorkStatus
{
    NotStarted,
    InProgress,
    OnHold,
    Completed,
    Cancelled
}