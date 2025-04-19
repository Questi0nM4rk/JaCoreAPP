using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.Dtos.Device;

/// <summary>
/// DTO for representing a DeviceCard.
/// </summary>
public class DeviceCardDto
{
    public int Id { get; set; }
    
    // Maybe include DeviceId here for reference?
    // public int DeviceId { get; set; } 
    
    public string? SerialNumber { get; set; }
    public DateTimeOffset? DateOfActivation { get; set; }
    
    public int? SupplierId { get; set; }
    // Include SupplierDto? For simplicity, keeping it as ID for now.
    
    public int? ServiceId { get; set; }
    // Include ServiceDto?
    
    // Metrological Conformation Levels
    public string? MetConLevel1 { get; set; }
    public string? MetConLevel2 { get; set; }
    public string? MetConLevel3 { get; set; }
    public string? MetConLevel4 { get; set; }
    
    // Consider including list of EventDtos or DeviceOperationDtos if needed often
    // public ICollection<EventDto> Events { get; set; } = new List<EventDto>();
} 