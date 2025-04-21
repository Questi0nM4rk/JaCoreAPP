// using JaCore.ApiO.Models.Enums; // Removed this line
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JaCore.Api.Models.Device;

public class DeviceOperation
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public int OrderIndex { get; set; }
    
    public int DeviceCardId { get; set; }
    
    [ForeignKey("DeviceCardId")]
    public DeviceCard? DeviceCard { get; set; }
    
    public bool IsRequired { get; set; }
    
    public bool IsCompleted { get; set; }
    
    [MaxLength(2000)]
    public string? UiElements { get; set; } // Stored as JSON

    // --- Added for relationship --- 
    public int DeviceId { get; set; } // Foreign Key
    public Device Device { get; set; } = null!; // Navigation Property
} 