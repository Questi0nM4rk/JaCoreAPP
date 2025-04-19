using System;
using JaCore.Common.Device;

namespace JaCoreUI.Models.Device;

public class Event
{
    public int Id { get; set; }
    public EventType? Type { get; set; }
    public string? Who { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public string? Description { get; set; }
    
    public Event() { }
    
    public Event(Event source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        
        Id = source.Id;
        Type = source.Type;
        Who = source.Who;
        From = source.From;
        To = source.To;
        Description = source.Description;
    }
    
    public Event Clone() => new Event(this);
}