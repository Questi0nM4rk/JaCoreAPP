namespace JaCore.Api.Dtos.Device;

/// <summary>
/// DTO for representing a DeviceOperation.
/// </summary>
public class DeviceOperationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public int DeviceCardId { get; set; }
    public bool IsRequired { get; set; }
    public bool IsCompleted { get; set; }
    public string? UiElements { get; set; }
} 