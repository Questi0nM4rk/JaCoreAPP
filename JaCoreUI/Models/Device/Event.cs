using System;

namespace JaCoreUI.Models.Device;

public class Event
{
    public required int Id { get; set; }

    public required EventType Type { get; set; }

    public required string Who { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }
    public string? Description { get; set; }
}

public enum EventType
{
    Maintenance,
    Malfunction,
    Operation,
    Service,
    Calibration
}