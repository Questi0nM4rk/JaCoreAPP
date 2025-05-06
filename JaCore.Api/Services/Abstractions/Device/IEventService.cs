using JaCore.Api.DTOs.Device;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JaCore.Api.Services.Abstractions.Device;

public interface IEventService
{
    Task<EventReadDto?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventReadDto>> GetAllEventsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<EventReadDto>> GetEventsByDeviceCardIdAsync(Guid deviceCardId, CancellationToken cancellationToken = default);
    Task<EventReadDto?> CreateEventAsync(EventCreateDto createDto, CancellationToken cancellationToken = default);
    // Delete might be relevant depending on requirements
    Task<bool> DeleteEventAsync(Guid id, CancellationToken cancellationToken = default);
} 