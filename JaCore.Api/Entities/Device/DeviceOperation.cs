namespace JaCore.Api.Entities.Device;

public class DeviceOperation : BaseEntity // Renamed from Operation to avoid ambiguity
{
    // Foreign Key for DeviceCard
    public Guid DeviceCardId { get; set; }
    // Navigation Property back to DeviceCard
    public virtual DeviceCard DeviceCard { get; set; } = null!;

    public string OperationType { get; set; } = string.Empty; // e.g., "Firmware Update", "Calibration Check"
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public string Status { get; set; } = string.Empty; // e.g., "Started", "Completed", "Failed"
    public string? Operator { get; set; } // User performing the operation
    public string? ResultsJson { get; set; } // Operation results/logs as JSON
} 