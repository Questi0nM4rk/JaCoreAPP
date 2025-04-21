using System.ComponentModel.DataAnnotations;
using JaCore.Common.Device;

namespace JaCore.Api.Models.Device;

public class Device
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
    
    public DeviceDataState DataState { get; set; }
    
    public DeviceOperationalState OperationalState { get; set; }
    
    public DateTimeOffset? CreatedAt { get; set; }
    
    public DateTimeOffset? ModifiedAt { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    
    public int? DeviceCardId { get; set; }
    public DeviceCard? DeviceCard { get; set; }

    [MaxLength(1000)]
    public string? Properties { get; set; } // Stored as JSON
    
    public int OrderIndex { get; set; }
    
    public bool IsCompleted { get; set; }
    
    // Navigation property
    public ICollection<DeviceOperation> DeviceOperations { get; set; } = new List<DeviceOperation>();
} 