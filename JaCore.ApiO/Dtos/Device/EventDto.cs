using JaCore.Common.Device;

namespace JaCore.Api.Dtos.Device;

/// <summary>
/// DTO for representing an Event.
/// </summary>
public class EventDto
{
    public int Id { get; set; }
    public EventType? Type { get; set; }
    public string? Who { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public string? Description { get; set; }
    public int DeviceCardId { get; set; }
}

/// <summary>
/// DTO for creating an Event.
/// </summary>
public class CreateEventDto
{
    public EventType? Type { get; set; }
    public string? Who { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public string? Description { get; set; }
    public int DeviceCardId { get; set; } // Required: Event must belong to a DeviceCard
}

/// <summary>
/// DTO for updating an Event.
/// </summary>
public class UpdateEventDto
{
    public EventType? Type { get; set; }
    public string? Who { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public string? Description { get; set; }
    // DeviceCardId is usually not updatable for an existing event
} 