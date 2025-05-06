using AutoMapper;
using AutoMapper.QueryableExtensions;
using JaCore.Api.Data;
using JaCore.Api.DTOs.Device;
using JaCore.Api.Entities.Device;
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

public class DeviceOperationService : BaseDeviceService, IDeviceOperationService
{
    private readonly IDeviceOperationRepository _repository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IMapper _mapper;

    public DeviceOperationService(
        IDeviceOperationRepository repository,
        IDeviceRepository deviceRepository,
        ApplicationDbContext context,
        ILogger<DeviceOperationService> logger,
        IMapper mapper) : base(logger, context)
    {
        _repository = repository;
        _deviceRepository = deviceRepository;
        _mapper = mapper;
    }

    public async Task<DeviceOperationReadDto?> CreateOperationAsync(DeviceOperationCreateDto createDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new device operation for CardId: {DeviceCardId}", createDto.DeviceCardId);
        try
        {
            if (!await _deviceRepository.ExistsAsync(d => d.Id == createDto.DeviceCardId, cancellationToken))
            {
                _logger.LogWarning("Device operation creation failed: Device with ID {DeviceCardId} not found.", createDto.DeviceCardId);
                return null;
            }

            var deviceOperation = _mapper.Map<DeviceOperation>(createDto);
            await _repository.AddAsync(deviceOperation, cancellationToken);
            bool saved = await SaveChangesAsync(cancellationToken);

            if (!saved) { /* log */ return null; }

            _logger.LogInformation("Successfully created device operation {OperationId} for card {DeviceCardId}", deviceOperation.Id, deviceOperation.DeviceCardId);
            return _mapper.Map<DeviceOperationReadDto>(deviceOperation);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error during device operation creation for CardId {DeviceCardId}", createDto.DeviceCardId);
            return null;
        }
    }

    public async Task<bool> DeleteOperationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to delete device operation with ID: {OperationId}", id);
        try
        {
            var deviceOperation = await _repository.GetByIdAsync(id, cancellationToken);
            if (deviceOperation == null)
            {
                _logger.LogWarning("Device operation deletion failed: Operation not found with ID {OperationId}.", id);
                return false;
            }

            _repository.Remove(deviceOperation);
            bool saved = await SaveChangesAsync(cancellationToken);
            if (!saved)
            {
                _logger.LogError("Failed to save deletion for DeviceOperation {OperationId}", id);
            }
            else
            {
                _logger.LogInformation("Successfully deleted device operation {OperationId}", id);
            }
            return saved;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error during DeviceOperation deletion process for ID {OperationId}", id);
            return false;
        }
    }

    public async Task<IEnumerable<DeviceOperationReadDto>> GetAllDeviceOperationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching all device operations");
        try
        {
            return await _repository.GetQueryable()
                                    .ProjectTo<DeviceOperationReadDto>(_mapper.ConfigurationProvider)
                                    .ToListAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving all DeviceOperations.");
            return Enumerable.Empty<DeviceOperationReadDto>();
        }
    }

    public async Task<DeviceOperationReadDto?> GetDeviceOperationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching device operation by ID: {OperationId}", id);
        try
        {
            return await _repository.GetQueryable()
                                    .Where(op => op.Id == id)
                                    .ProjectTo<DeviceOperationReadDto>(_mapper.ConfigurationProvider)
                                    .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving device operation with ID {OperationId}", id);
            return null;
        }
    }

    public async Task<IEnumerable<DeviceOperationReadDto>> GetDeviceOperationsByDeviceIdAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching device operations for DeviceId: {DeviceId}", deviceId);
        try
        {
            return await _repository.GetQueryable()
                                    .Where(op => op.DeviceCardId == deviceId)
                                    .ProjectTo<DeviceOperationReadDto>(_mapper.ConfigurationProvider)
                                    .ToListAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving device operations for device ID {DeviceId}", deviceId);
            return Enumerable.Empty<DeviceOperationReadDto>();
        }
    }

    public async Task<bool> UpdateOperationAsync(Guid id, DeviceOperationUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating DeviceOperation with ID: {OperationId}", id);
        try
        {
            var deviceOperation = await _repository.GetByIdAsync(id, cancellationToken);
            if (deviceOperation == null)
            {
                _logger.LogWarning("DeviceOperation update failed: Operation not found with ID {OperationId}.", id);
                return false;
            }

            _mapper.Map(updateDto, deviceOperation);
            _repository.Update(deviceOperation);
            bool saved = await SaveChangesAsync(cancellationToken);

            if (!saved)
            {
                _logger.LogError("Failed to save update for DeviceOperation {OperationId}", id);
            }
            else
            {
                _logger.LogInformation("Successfully updated DeviceOperation {OperationId}", id);
            }
            return saved;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error during DeviceOperation update process for ID {OperationId}", id);
            return false;
        }
    }

    public async Task<IEnumerable<DeviceOperationReadDto>> GetOperationsByDeviceCardIdAsync(Guid deviceCardId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching operations for CardId: {CardId}", deviceCardId);
        try
        {
            return await _repository.GetQueryable()
                                    .Where(op => op.DeviceCardId == deviceCardId)
                                    .ProjectTo<DeviceOperationReadDto>(_mapper.ConfigurationProvider)
                                    .ToListAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving operations for card ID {CardId}", deviceCardId);
            return Enumerable.Empty<DeviceOperationReadDto>();
        }
    }

    public async Task<DeviceOperationReadDto?> GetOperationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching operation by ID: {OperationId}", id);
        try
        {
            return await _repository.GetQueryable()
                                    .Where(op => op.Id == id)
                                    .ProjectTo<DeviceOperationReadDto>(_mapper.ConfigurationProvider)
                                    .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving operation with ID {OperationId}", id);
            return null;
        }
    }
} 