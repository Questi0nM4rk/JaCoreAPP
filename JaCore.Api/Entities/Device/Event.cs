using JaCore.Common.Device;

namespace JaCore.Api.Entities.Device;

public class Event : BaseEntity
{
    // Foreign Key for DeviceCard
    public Guid DeviceCardId { get; set; }
    // Navigation Property back to DeviceCard
    public virtual DeviceCard DeviceCard { get; set; } = null!;

    public EventType Type { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? DetailsJson { get; set; } // Additional details as JSON
    public string? TriggeredBy { get; set; } // e.g., User ID, System Process
} 