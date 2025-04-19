using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Interfaces.Services;
using JaCore.Api.Models.Device;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JaCore.Api.Services.Device;

/// <summary>
/// Service implementation for managing Event entities.
/// </summary>
public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IDeviceCardRepository _cardRepository; // Needed for validation
    private readonly ILogger<EventService> _logger;

    public EventService(
        IEventRepository eventRepository,
        IDeviceCardRepository cardRepository, // Inject card repo
        ILogger<EventService> logger)
    {
        _eventRepository = eventRepository;
        _cardRepository = cardRepository;
        _logger = logger;
    }

    public async Task<PaginatedListDto<EventDto>> GetByDeviceCardIdAsync(int deviceCardId, int pageNumber, int pageSize)
    {
         _logger.LogInformation("Getting Events for DeviceCardId: {DeviceCardId}", deviceCardId);
        
        // Validate DeviceCard exists
        var cardExists = await _cardRepository.ExistsAsync(deviceCardId);
        if (!cardExists)
        {
            _logger.LogWarning("DeviceCard with Id {DeviceCardId} not found when getting events.", deviceCardId);
            // Return empty list as events for a non-existent card is just an empty set
            return new PaginatedListDto<EventDto>(Enumerable.Empty<EventDto>(), 0, pageNumber, pageSize);
        }
        
        var paginatedEvents = await _eventRepository.GetByDeviceCardIdAsync(deviceCardId, pageNumber, pageSize);
        var eventDtos = paginatedEvents.Items.Select(ev => MapToDto(ev));
        
        return new PaginatedListDto<EventDto>(eventDtos, paginatedEvents.TotalCount, pageNumber, pageSize);
    }

    public async Task<EventDto?> GetByIdAsync(int id)
    {
        var ev = await _eventRepository.GetByIdAsync(id);
        return ev == null ? null : MapToDto(ev);
    }

    public async Task<EventDto> CreateAsync(CreateEventDto createDto)
    {
        _logger.LogInformation("Attempting to create Event for DeviceCardId: {DeviceCardId}", createDto.DeviceCardId);

        // 1. Validate DeviceCard exists
        var cardExists = await _cardRepository.ExistsAsync(createDto.DeviceCardId);
        if (!cardExists)
        {
             _logger.LogWarning("DeviceCard with Id {DeviceCardId} not found for creating Event.", createDto.DeviceCardId);
            throw new ArgumentException($"DeviceCard with Id {createDto.DeviceCardId} not found.", nameof(createDto.DeviceCardId));
        }
        
        // 2. Validate DTO fields (e.g., date ranges)
        if (createDto.From.HasValue && createDto.To.HasValue && createDto.From > createDto.To)
        {
             throw new ArgumentException("'From' date cannot be after 'To' date.", nameof(createDto.From));
        }

        // Add validation for Description
        if (string.IsNullOrWhiteSpace(createDto.Description))
        {
            _logger.LogWarning("Attempted to create event with null or empty description for DeviceCardId {DeviceCardId}.", createDto.DeviceCardId);
            throw new ArgumentException("Description cannot be null, empty, or whitespace.", nameof(createDto.Description));
        }

        var newEvent = new Event
        {
            Type = createDto.Type,
            Who = createDto.Who,
            From = createDto.From,
            To = createDto.To,
            Description = createDto.Description,
            DeviceCardId = createDto.DeviceCardId // Set FK
        };

        try
        {
            var addedEvent = await _eventRepository.AddAsync(newEvent);
            _logger.LogInformation("Successfully created Event {EventId} for DeviceCardId: {DeviceCardId}", addedEvent.Id, addedEvent.DeviceCardId);
            return MapToDto(addedEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating Event for DeviceCardId: {DeviceCardId}", createDto.DeviceCardId);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(int id, UpdateEventDto updateDto)
    {
         _logger.LogInformation("Attempting to update Event with Id: {EventId}", id);
        var existingEvent = await _eventRepository.GetByIdAsync(id);
        if (existingEvent == null)
        {
             _logger.LogWarning("Event with Id {EventId} not found for update.", id);
            return false;
        }
        
        // Validate DTO fields (e.g., date ranges)
        if (updateDto.From.HasValue && updateDto.To.HasValue && updateDto.From > updateDto.To)
        {
             throw new ArgumentException("'From' date cannot be after 'To' date.", nameof(updateDto.From));
        }
        
        // Map fields (don't update DeviceCardId)
        existingEvent.Type = updateDto.Type;
        existingEvent.Who = updateDto.Who;
        existingEvent.From = updateDto.From;
        existingEvent.To = updateDto.To;
        existingEvent.Description = updateDto.Description;
        
        try
        {
            await _eventRepository.UpdateAsync(existingEvent);
             _logger.LogInformation("Successfully updated Event {EventId}", id);
            return true;
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error occurred while updating Event {EventId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Attempting to delete Event with Id: {EventId}", id);
        var existingEvent = await _eventRepository.GetByIdAsync(id);
        if (existingEvent == null)
        {
            _logger.LogWarning("Event with Id {EventId} not found for deletion.", id);
            return false;
        }

        try
        {
            await _eventRepository.DeleteAsync(id);
             _logger.LogInformation("Successfully deleted Event {EventId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting Event {EventId}", id);
            throw;
        }
    }
    
    // --- Helper Methods ---
    
    private static EventDto MapToDto(Event ev)
    {
        return new EventDto
        {
            Id = ev.Id,
            Type = ev.Type,
            Who = ev.Who,
            From = ev.From,
            To = ev.To,
            Description = ev.Description,
            DeviceCardId = ev.DeviceCardId
        };
    }
} 