using JaCore.Api.Entities.Device; // Correct namespace where BaseEntity was found
using JaCore.Common.Device; // For EventType enum
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JaCore.Api.Entities.Device;

[Table("DeviceEvents")]
public class DeviceEvent : BaseEntity
{
    [Required]
    public Guid CardId { get; set; }

    [ForeignKey(nameof(CardId))]
    public virtual DeviceCard? DeviceCard { get; set; }

    [Required]
    public EventType Type { get; set; }

    [Required]
    public DateTimeOffset Timestamp { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")] // Assuming PostgreSQL for JSON storage
    public string? DetailsJson { get; set; }

    [MaxLength(100)]
    public string? TriggeredBy { get; set; } // e.g., User ID, System Process
} 