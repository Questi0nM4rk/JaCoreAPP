using JaCore.Common.Device;

namespace JaCore.Api.Entities.Device;

public class DeviceCard : BaseEntity
{
    // Foreign Key for the Device (One-to-One relationship)
    public Guid DeviceId { get; set; }
    // Navigation Property back to the Device
    public virtual Device Device { get; set; } = null!; // Required relationship

    public string Location { get; set; } = string.Empty;
    public string? AssignedUser { get; set; } // Could be linked to ApplicationUser later
    public DateTimeOffset LastSeenAt { get; set; }
    public DeviceDataState DataState { get; set; } = DeviceDataState.Idle;
    public DeviceOperationalState OperationalState { get; set; } = DeviceOperationalState.Unknown;

    // Example complex property stored as JSON string (configure in DbContext if needed)
    public string? PropertiesJson { get; set; } // Holds dynamic properties

    // Navigation property for related events
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    // Navigation property for related operations
    public virtual ICollection<DeviceOperation> Operations { get; set; } = new List<DeviceOperation>();
} 