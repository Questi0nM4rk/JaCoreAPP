using JaCore.Api.DTOs.Device;

namespace JaCore.Api.Services.Abstractions.Device;

public interface IDeviceEventService
{
    // Placeholder methods - based on common patterns and EventDtos.cs
    Task<EventReadDto?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventReadDto>> GetAllEventsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<EventReadDto>> GetEventsByDeviceCardIdAsync(Guid deviceCardId, CancellationToken cancellationToken = default);
    Task<EventReadDto?> CreateEventAsync(EventCreateDto createDto, CancellationToken cancellationToken = default);
    // Typically events are not updated or deleted, but placeholders if needed:
    // Task<bool> UpdateEventAsync(Guid id, EventUpdateDto updateDto, CancellationToken cancellationToken = default);
    // Task<bool> DeleteEventAsync(Guid id, CancellationToken cancellationToken = default);
} 