using AutoMapper;
using AutoMapper.QueryableExtensions;
using JaCore.Api.Data;
using JaCore.Api.DTOs.Device;
using JaCore.Api.Entities.Device;
using JaCore.Api.Services.Abstractions.Device;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JaCore.Api.Repositories.Abstactions.Device;
using JaCore.Api.Repositories.Device;

namespace JaCore.Api.Services.Device;

public class DeviceEventService : BaseDeviceService, Abstractions.Device.IDeviceEventService
{
    private readonly IDeviceEventRepository _repository;
    private readonly IDeviceCardRepository _cardRepository;
    private readonly IMapper _mapper;

    public DeviceEventService(
        IDeviceEventRepository repository,
        IDeviceCardRepository cardRepository,
        ApplicationDbContext context,
        ILogger<DeviceEventService> logger,
        IMapper mapper) : base(logger, context)
    {
        _repository = repository;
        _cardRepository = cardRepository;
        _mapper = mapper;
    }

    public async Task<EventReadDto?> CreateEventAsync(EventCreateDto createDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new device event for CardId: {DeviceCardId}", createDto.DeviceCardId);
        try
        {
            if (!await _cardRepository.ExistsAsync(dc => dc.Id == createDto.DeviceCardId, cancellationToken))
            {
                _logger.LogWarning("Device event creation failed: Card with ID {DeviceCardId} not found.", createDto.DeviceCardId);
                return null;
            }

            var deviceEvent = _mapper.Map<DeviceEvent>(createDto);
            await _repository.AddAsync(deviceEvent, cancellationToken);
            bool saved = await SaveChangesAsync(cancellationToken);

            if (!saved) { /* log */ return null; }

            _logger.LogInformation("Successfully created device event {EventId} for card {DeviceCardId}", deviceEvent.Id, deviceEvent.CardId);
            return _mapper.Map<EventReadDto>(deviceEvent);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error during device event creation for CardId {DeviceCardId}", createDto.DeviceCardId);
            return null;
        }
    }

    public async Task<bool> DeleteEventAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to delete device event with ID: {EventId}", id);
        try
        {
            var deviceEvent = await _repository.GetByIdAsync(id, cancellationToken);
            if (deviceEvent == null)
            {
                _logger.LogWarning("Device event deletion failed: Event not found with ID {EventId}.", id);
                return false;
            }

            _repository.Remove(deviceEvent);
            bool saved = await SaveChangesAsync(cancellationToken);
            if (!saved)
            {
                _logger.LogError("Failed to save deletion for DeviceEvent {EventId}", id);
            }
            else
            {
                _logger.LogInformation("Successfully deleted device event {EventId}", id);
            }
            return saved;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error during DeviceEvent deletion process for ID {EventId}", id);
            return false;
        }
    }

    public async Task<IEnumerable<EventReadDto>> GetAllEventsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching all device events");
        try
        {
            return await _repository.GetAllQueryable()
                                    .ProjectTo<EventReadDto>(_mapper.ConfigurationProvider)
                                    .ToListAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving all DeviceEvents.");
            return Enumerable.Empty<EventReadDto>();
        }
    }

    public async Task<EventReadDto?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching device event by ID: {EventId}", id);
        try
        {
            return await _repository.GetAllQueryable()
                                    .Where(de => de.Id == id)
                                    .ProjectTo<EventReadDto>(_mapper.ConfigurationProvider)
                                    .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving device event with ID {EventId}", id);
            return null;
        }
    }

    public async Task<IEnumerable<EventReadDto>> GetEventsByDeviceCardIdAsync(Guid cardId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching device events for CardId: {CardId}", cardId);
        try
        {
            return await _repository.GetAllQueryable()
                                    .Where(de => de.CardId == cardId)
                                    .ProjectTo<EventReadDto>(_mapper.ConfigurationProvider)
                                    .ToListAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving device events for card ID {CardId}", cardId);
            return Enumerable.Empty<EventReadDto>();
        }
    }
} 