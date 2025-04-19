using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.Models.Device;

public class Supplier
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Contact { get; set; }
    
    // Navigation property
    public ICollection<DeviceCard> DeviceCards { get; set; } = new List<DeviceCard>();
} 