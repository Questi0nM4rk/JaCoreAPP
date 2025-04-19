using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JaCore.Common.Device;

namespace JaCore.Api.Models.Device;

public class Event
{
    [Key]
    public int Id { get; set; }
    
    public EventType? Type { get; set; }
    
    [MaxLength(100)]
    public string? Who { get; set; }
    
    public DateTimeOffset? From { get; set; }
    
    public DateTimeOffset? To { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public int DeviceCardId { get; set; }
    
    [ForeignKey("DeviceCardId")]
    public DeviceCard? DeviceCard { get; set; }
} 