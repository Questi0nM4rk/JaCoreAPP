namespace JaCoreUI.Models.Production;

/// <summary>
/// Defines the operational modes for the production guide UI.
/// </summary>
public enum WorkflowMode
{
    /// <summary>
    /// Designing or editing a reusable production template.
    /// </summary>
    Template,

    /// <summary>
    /// Configuring a specific production run instance from a template.
    /// </summary>
    Prepare,

    /// <summary>
    /// Executing a prepared production run and recording data.
    /// </summary>
    Work
} 