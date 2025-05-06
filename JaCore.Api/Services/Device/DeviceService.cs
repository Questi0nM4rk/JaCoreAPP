using AutoMapper;
using AutoMapper.QueryableExtensions;
using JaCore.Api.Data;
using JaCore.Api.DTOs.Device;
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

public class DeviceService : BaseDeviceService, IDeviceService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IMapper _mapper;

    public DeviceService(
        IDeviceRepository deviceRepository,
        ApplicationDbContext context,
        ILogger<DeviceService> logger,
        IMapper mapper) : base(logger, context)
    {
        _deviceRepository = deviceRepository;
        _mapper = mapper;
    }

    public async Task<DeviceDto?> CreateDeviceAsync(DeviceCreateDto createDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new device: {DeviceName}", createDto.Name);
        try
        {
            if (await _deviceRepository.ExistsAsync(d => d.SerialNumber == createDto.SerialNumber, cancellationToken))
            {
                _logger.LogWarning("Device creation failed: Serial number {SerialNumber} already exists.", createDto.SerialNumber);
                return null;
            }

            var device = _mapper.Map<Entities.Device.Device>(createDto);

            await _deviceRepository.AddAsync(device, cancellationToken);
            bool saved = await SaveChangesAsync(cancellationToken);

            if (!saved) { /* Log handled in SaveChangesAsync */ return null; }

            _logger.LogInformation("Successfully created device {DeviceName} with ID {DeviceId}", device.Name, device.Id);
            return _mapper.Map<DeviceDto>(device);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error during device creation process for {DeviceName}", createDto.Name);
            return null;
        }
    }

    public async Task<bool> DeleteDeviceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to delete device with ID: {DeviceId}", id);
        try
        {
            var device = await _deviceRepository.GetByIdAsync(id, cancellationToken, includes: d => d.DeviceCard!);
            if (device == null)
            {
                _logger.LogWarning("Device deletion failed: Device with ID {DeviceId} not found.", id);
                return false;
            }

            if (device.DeviceCard != null)
            {
                _logger.LogWarning("Device deletion failed for ID {DeviceId}: Associated device card exists.", id);
                return false;
            }

            _deviceRepository.Remove(device);
            bool saved = await SaveChangesAsync(cancellationToken);
            if (!saved)
            {
                _logger.LogError("Failed to save deletion for device {DeviceId}", id);
            }
            else
            {
                _logger.LogInformation("Successfully deleted device {DeviceId}", id);
            }
            return saved;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error during device deletion process for ID {DeviceId}", id);
            return false;
        }
    }

    public async Task<IEnumerable<DeviceDto>> GetAllDevicesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching all devices");
        try
        {
            return await _deviceRepository.GetQueryable()
                         .Include(d => d.Category)
                         .Include(d => d.Supplier)
                         .ProjectTo<DeviceDto>(_mapper.ConfigurationProvider)
                         .ToListAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving all devices");
            return Enumerable.Empty<DeviceDto>();
        }
    }

    public async Task<DeviceDto?> GetDeviceByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching device by ID: {DeviceId}", id);
        try
        {
            var deviceDto = await _deviceRepository.GetQueryable()
                                   .Where(d => d.Id == id)
                                   .Include(d => d.Category)
                                   .Include(d => d.Supplier)
                                   .ProjectTo<DeviceDto>(_mapper.ConfigurationProvider)
                                   .FirstOrDefaultAsync(cancellationToken);
            return deviceDto;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving device with ID {DeviceId}", id);
            return null;
        }
    }

    public async Task<DeviceDto?> GetDeviceBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching device by Serial Number: {SerialNumber}", serialNumber);
        try
        {
            var deviceDto = await _deviceRepository.GetQueryable()
                                   .Where(d => d.SerialNumber == serialNumber)
                                   .Include(d => d.Category)
                                   .Include(d => d.Supplier)
                                   .ProjectTo<DeviceDto>(_mapper.ConfigurationProvider)
                                   .FirstOrDefaultAsync(cancellationToken);
            return deviceDto;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving device with Serial Number {SerialNumber}", serialNumber);
            return null;
        }
    }

    public async Task<bool> UpdateDeviceAsync(Guid id, DeviceUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to update device with ID: {DeviceId}", id);
        try
        {
            var device = await _deviceRepository.GetByIdAsync(id, cancellationToken);
            if (device == null)
            {
                _logger.LogWarning("Device update failed: Device with ID {DeviceId} not found.", id);
                return false;
            }

            _mapper.Map(updateDto, device);

            _deviceRepository.Update(device);
            bool saved = await SaveChangesAsync(cancellationToken);

            if (!saved) { /* Log handled in SaveChangesAsync */ return false; }

            _logger.LogInformation("Successfully updated device {DeviceId}", id);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error during device update process for ID {DeviceId}", id);
            return false;
        }
    }
} 