using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.Models.Device;

public class DeviceCard
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(100)]
    public string? SerialNumber { get; set; }
    
    public DateTimeOffset? DateOfActivation { get; set; }
    
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    
    public int? ServiceId { get; set; }
    public Service? Service { get; set; }
    
    // --- Added for relationship --- 
    public int? MetrologicalConformationId { get; set; } // Foreign Key (Nullable)
    public MetrologicalConformation? MetrologicalConformation { get; set; } // Navigation Property (Nullable)
    
    // Added Metrological Conformation Levels directly
    [MaxLength(100)]
    public string? MetConLevel1 { get; set; }
    [MaxLength(100)]
    public string? MetConLevel2 { get; set; }
    [MaxLength(100)]
    public string? MetConLevel3 { get; set; }
    [MaxLength(100)]
    public string? MetConLevel4 { get; set; }
    
    // Navigation property to Device (owning side)
    public Device? Device { get; set; }
    
    // Navigation property to Events
    public ICollection<Event> Events { get; set; } = new List<Event>();
} 