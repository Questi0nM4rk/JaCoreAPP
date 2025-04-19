using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.Dtos.Device;

/// <summary>
/// DTO for updating an existing DeviceOperation.
/// </summary>
public class UpdateDeviceOperationDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public int OrderIndex { get; set; }
    
    // DeviceCardId should typically not be updatable here
    // public int DeviceCardId { get; set; } 
    
    public bool IsRequired { get; set; }
    
    public bool IsCompleted { get; set; }
    
    [MaxLength(2000)]
    public string? UiElements { get; set; } // Stored as JSON
} 