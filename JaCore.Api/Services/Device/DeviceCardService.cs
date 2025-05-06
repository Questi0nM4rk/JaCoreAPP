using AutoMapper;
using AutoMapper.QueryableExtensions;
using JaCore.Api.Data;
using JaCore.Api.DTOs.Device;
using JaCore.Api.Entities.Device; // Needed for DeviceCard entity
using JaCore.Api.Services.Abstractions.Device;
using JaCore.Api.Services.Repositories.Device;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JaCore.Api.Services.Device;

public class DeviceCardService : BaseDeviceService, Abstractions.Device.IDeviceCardService
{
    private readonly Repositories.Device.IDeviceCardRepository _repository;
    private readonly Repositories.Device.IDeviceRepository _deviceRepository;
    private readonly IMapper _mapper;

    public DeviceCardService(
        Repositories.Device.IDeviceCardRepository repository,
        Repositories.Device.IDeviceRepository deviceRepository,
        ApplicationDbContext context,
        ILogger<DeviceCardService> logger,
        IMapper mapper) : base(logger, context)
    {
        _repository = repository;
        _deviceRepository = deviceRepository;
        _mapper = mapper;
    }

    public async Task<DeviceCardReadDto?> CreateDeviceCardAsync(DeviceCardCreateDto createDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new device card for DeviceId: {DeviceId}", createDto.DeviceId);
        try
        {
            if (!await _deviceRepository.ExistsAsync(d => d.Id == createDto.DeviceId, cancellationToken))
            {
                _logger.LogWarning("Device card creation failed: Device with ID {DeviceId} not found.", createDto.DeviceId);
                return null;
            }
            if (await _repository.ExistsAsync(dc => dc.DeviceId == createDto.DeviceId, cancellationToken))
            {
                _logger.LogWarning("Device card creation failed: Card already exists for Device ID {DeviceId}.", createDto.DeviceId);
                return null;
            }

            var card = _mapper.Map<DeviceCard>(createDto);
            card.LastSeenAt = DateTimeOffset.UtcNow;

            await _repository.AddAsync(card, cancellationToken);
            bool saved = await SaveChangesAsync(cancellationToken);

            if (!saved) { /* log */ return null; }

            _logger.LogInformation("Successfully created device card {CardId} for device {DeviceId}", card.Id, card.DeviceId);
            return _mapper.Map<DeviceCardReadDto>(card);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error during device card creation for DeviceId {DeviceId}", createDto.DeviceId);
            return null;
        }
    }

    public async Task<bool> DeleteDeviceCardAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to delete device card with ID: {CardId}", id);
        try
        {
            // Correct parameter name from includeProperties to includes
            // Also, simplify includes - checking Any() requires fetching data first or complex expression.
            // Fetching first to check dependencies.
            var card = await _repository.GetByIdAsync(id, cancellationToken);
            if (card == null)
            {
                _logger.LogWarning("Device card deletion failed: Card not found with ID {CardId}.", id);
                return false;
            }

            // Check dependencies after fetching (Requires DbContext tracking or separate queries)
            // This logic might need refinement based on actual DbContext setup
            bool hasDependencies = await _context.DeviceEvents.AnyAsync(e => e.CardId == id, cancellationToken) ||
                                   await _context.DeviceOperations.AnyAsync(o => o.DeviceCardId == id, cancellationToken);

            if (hasDependencies)
            {
                _logger.LogWarning("Device card deletion failed for ID {CardId}: Associated events or operations exist.", id);
                return false; // Prevent deletion due to dependencies
            }

            _repository.Remove(card);
            bool saved = await SaveChangesAsync(cancellationToken);

            if (!saved)
            {
                _logger.LogError("Failed to save deletion for DeviceCard {CardId}", id);
            }
            else
            {
                _logger.LogInformation("Successfully deleted device card {CardId}", id);
            }
            return saved;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error during DeviceCard deletion process for ID {CardId}", id);
            return false;
        }
    }

    public async Task<IEnumerable<DeviceCardReadDto>> GetAllDeviceCardsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching all device cards");
        try
        {
            return await _repository.GetQueryable()
                         .ProjectTo<DeviceCardReadDto>(_mapper.ConfigurationProvider)
                         .ToListAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving all DeviceCards.");
            return Enumerable.Empty<DeviceCardReadDto>();
        }
    }

    public async Task<DeviceCardReadDto?> GetDeviceCardByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching device card by ID: {CardId}", id);
        try
        {
            return await _repository.GetQueryable()
                         .Where(dc => dc.Id == id)
                         .ProjectTo<DeviceCardReadDto>(_mapper.ConfigurationProvider)
                         .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving device card with ID {CardId}", id);
            return null;
        }
    }

    public async Task<IEnumerable<DeviceCardReadDto>> GetDeviceCardsByDeviceIdAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching device cards for DeviceId: {DeviceId}", deviceId);
        try
        {
            return await _repository.GetQueryable()
                         .Where(dc => dc.DeviceId == deviceId)
                         .ProjectTo<DeviceCardReadDto>(_mapper.ConfigurationProvider)
                         .ToListAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving device cards for device ID {DeviceId}", deviceId);
            return Enumerable.Empty<DeviceCardReadDto>();
        }
    }

    public async Task<bool> UpdateDeviceCardAsync(Guid id, DeviceCardUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating DeviceCard with ID: {CardId}", id);
        try
        {
            var card = await _repository.GetByIdAsync(id, cancellationToken);
            if (card == null)
            {
                _logger.LogWarning("DeviceCard update failed: Card not found with ID {CardId}.", id);
                return false;
            }

            _mapper.Map(updateDto, card);

            _repository.Update(card);
            bool saved = await SaveChangesAsync(cancellationToken);

            if (!saved)
            {
                _logger.LogError("Failed to save update for DeviceCard {CardId}", id);
            }
            else
            {
                _logger.LogInformation("Successfully updated DeviceCard {CardId}", id);
            }
            return saved;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error during DeviceCard update process for ID {CardId}", id);
            return false;
        }
    }
} 