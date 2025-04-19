using JaCore.Common.Device;

namespace JaCore.Api.Dtos.Device;

/// <summary>
/// DTO for returning device data
/// </summary>
public class DeviceDto
{
    /// <summary>
    /// Unique identifier for the device
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the device
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the device
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Current data state of the device
    /// </summary>
    public DeviceDataState DataState { get; set; }

    /// <summary>
    /// Current operational state of the device
    /// </summary>
    public DeviceOperationalState OperationalState { get; set; }

    /// <summary>
    /// Date and time when the device was created
    /// </summary>
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the device was last modified
    /// </summary>
    public DateTimeOffset? ModifiedAt { get; set; }

    /// <summary>
    /// ID of the category the device belongs to
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// ID of the device card associated with this device (null if none)
    /// </summary>
    public int? DeviceCardId { get; set; }

    /// <summary>
    /// Additional properties stored as JSON
    /// </summary>
    public string? Properties { get; set; }

    /// <summary>
    /// Order index for sorting
    /// </summary>
    public int OrderIndex { get; set; }

    /// <summary>
    /// Indicates whether the device is completed
    /// </summary>
    public bool IsCompleted { get; set; }
}

/// <summary>
/// DTO for creating a new device
/// </summary>
public class CreateDeviceDto
{
    /// <summary>
    /// Name of the device
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the device
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Initial data state of the device
    /// </summary>
    public DeviceDataState DataState { get; set; }

    /// <summary>
    /// Initial operational state of the device
    /// </summary>
    public DeviceOperationalState OperationalState { get; set; }

    /// <summary>
    /// ID of the category the device belongs to
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Additional properties stored as JSON
    /// </summary>
    public string? Properties { get; set; }

    /// <summary>
    /// Order index for sorting
    /// </summary>
    public int OrderIndex { get; set; }

    /// <summary>
    /// Indicates whether the device is completed
    /// </summary>
    public bool IsCompleted { get; set; }
}

/// <summary>
/// DTO for updating an existing device
/// </summary>
public class UpdateDeviceDto
{
    /// <summary>
    /// Updated name of the device
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Updated description of the device
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Updated data state of the device
    /// </summary>
    public DeviceDataState DataState { get; set; }

    /// <summary>
    /// Updated operational state of the device
    /// </summary>
    public DeviceOperationalState OperationalState { get; set; }

    /// <summary>
    /// Updated category ID for the device
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Updated properties stored as JSON
    /// </summary>
    public string? Properties { get; set; }

    /// <summary>
    /// Updated order index for sorting
    /// </summary>
    public int OrderIndex { get; set; }

    /// <summary>
    /// Updated completion status
    /// </summary>
    public bool IsCompleted { get; set; }
} 