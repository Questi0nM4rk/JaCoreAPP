using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.Dtos.Device;

/// <summary>
/// DTO for updating an existing DeviceCard.
/// </summary>
public class UpdateDeviceCardDto
{
    // Which fields are updatable? Assuming most are.
    [MaxLength(100)]
    public string? SerialNumber { get; set; }
    
    public DateTimeOffset? DateOfActivation { get; set; }
    
    public int? SupplierId { get; set; }
    
    public int? ServiceId { get; set; }
    
    // Metrological Conformation Levels
    [MaxLength(100)]
    public string? MetConLevel1 { get; set; }
    [MaxLength(100)]
    public string? MetConLevel2 { get; set; }
    [MaxLength(100)]
    public string? MetConLevel3 { get; set; }
    [MaxLength(100)]
    public string? MetConLevel4 { get; set; }
} 