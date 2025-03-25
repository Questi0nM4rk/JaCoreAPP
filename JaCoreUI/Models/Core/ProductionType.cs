namespace JaCoreUI.Models.Core
{
    /// <summary>
    /// Defines the types of production that can exist in the system
    /// </summary>
    public enum ProductionType
    {
        /// <summary>
        /// Template production - used for defining reusable structures
        /// </summary>
        Template,
        
        /// <summary>
        /// Preparation production - used for customizing parameters
        /// </summary>
        Preparation,
        
        /// <summary>
        /// Work production - used for actual work execution
        /// </summary>
        Work
    }
}