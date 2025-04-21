using AutoMapper;
using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json; // For JSON validation
// Alias the model to avoid namespace conflicts
using DeviceModel = JaCore.Api.Models.Device.Device;

namespace JaCore.Api.Services.Device;

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly ILogger<DeviceService> _logger;
    // Inject other dependencies like ICategoryRepository or DbContext if needed for validation

    public DeviceService(IDeviceRepository deviceRepository, ILogger<DeviceService> logger)
    {
        _deviceRepository = deviceRepository;
        _logger = logger;
    }

    public async Task<PaginatedListDto<DeviceDto>> GetAllAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;
        
        // Repository already handles default sorting by ModifiedAt descending
        var totalCount = await _deviceRepository.CountAsync();
        var devices = await _deviceRepository.GetAllAsync(pageNumber, pageSize);
        
        // Manual mapping (Consider AutoMapper for more complex scenarios)
        var deviceDtos = devices.Select(d => MapToDto(d));
        
        return new PaginatedListDto<DeviceDto>(deviceDtos, totalCount, pageNumber, pageSize);
    }

    public async Task<DeviceDto?> GetByIdAsync(int id)
    {
        var device = await _deviceRepository.GetByIdAsync(id);
        return device == null ? null : MapToDto(device);
    }

    public async Task<DeviceDto> CreateAsync(CreateDeviceDto createDto)
    {
        // Add validation logic here (e.g., check if CategoryId exists)
        if (string.IsNullOrWhiteSpace(createDto.Name) || createDto.Name.Length > 100)
        {
            throw new ArgumentException("Device name is required and cannot exceed 100 characters.", nameof(createDto.Name));
        }
        // Add more validation as needed...

        // Validate Properties JSON
        if (!string.IsNullOrEmpty(createDto.Properties))
        {
            try { JsonDocument.Parse(createDto.Properties); }
            catch (JsonException ex)
            {
                throw new ArgumentException("Properties field must contain valid JSON.", nameof(createDto.Properties), ex);
            }
        }

        var device = new DeviceModel
        {
            Name = createDto.Name,
            Description = createDto.Description,
            DataState = createDto.DataState,
            OperationalState = createDto.OperationalState,
            CategoryId = createDto.CategoryId,
            // DeviceCardId is NO LONGER set here. It's managed by DeviceCardService.
            // DeviceCardId = createDto.DeviceCardId, 
            Properties = createDto.Properties,
            OrderIndex = createDto.OrderIndex,
            IsCompleted = createDto.IsCompleted
            // CreatedAt/ModifiedAt are set by Repository override
        };
        
        var addedDevice = await _deviceRepository.AddAsync(device);
        return MapToDto(addedDevice);
    }

    public async Task<bool> UpdateAsync(int id, UpdateDeviceDto updateDto)
    {
        var existingDevice = await _deviceRepository.GetByIdAsync(id);
        if (existingDevice == null)
        {
            _logger.LogWarning("Device with ID {DeviceId} not found for update", id);
            return false;
        }

        if (string.IsNullOrWhiteSpace(updateDto.Name))
        {
            _logger.LogError("Update failed: Device name cannot be null or whitespace.");
            throw new ArgumentException("Device name cannot be null or whitespace.", nameof(updateDto.Name));
        }

        existingDevice.Name = updateDto.Name;
        existingDevice.Description = updateDto.Description;
        existingDevice.DataState = updateDto.DataState;
        existingDevice.OperationalState = updateDto.OperationalState;
        existingDevice.CategoryId = updateDto.CategoryId;
        existingDevice.Properties = updateDto.Properties;
        existingDevice.OrderIndex = updateDto.OrderIndex;
        existingDevice.IsCompleted = updateDto.IsCompleted;

        try
        {
            await _deviceRepository.UpdateAsync(existingDevice);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict occurred while updating device {DeviceId}", id);
            if (!await _deviceRepository.ExistsAsync(id))
            {
                return false;
            }
            throw;
        }
        catch(Exception ex)
        {
             _logger.LogError(ex, "Error updating device {DeviceId}", id);
             throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var device = await _deviceRepository.GetByIdAsync(id);
        if (device == null)
        {
            return false;
        }

        // CRITICAL: Check if a DeviceCard is linked. If so, prevent deletion?
        // Or should deleting a Device cascade delete the Card (handled by DB constraints/service logic)?
        // Assuming for now that a Device with a Card cannot be deleted directly.
        if (device.DeviceCardId.HasValue)
        {
            _logger.LogWarning("Attempted to delete Device {DeviceId} which has an associated DeviceCard {DeviceCardId}. Deletion blocked.", id, device.DeviceCardId.Value);
            throw new InvalidOperationException($"Cannot delete Device {id} because it has an associated Device Card. Delete the Device Card first.");
        }
        
        // TODO: Add checks for linked DeviceOperations if they are still linked directly to Device
        // (They should now be linked to DeviceCard based on Step 3)

        try
        {
            await _deviceRepository.DeleteAsync(id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting device {DeviceId}", id);
            // Return false or rethrow?
            // Rethrowing for now to let controller handle 500
            throw; 
        }
    }

    // Simple mapper function (Consider AutoMapper for complex objects)
    private static DeviceDto MapToDto(DeviceModel device)
    {
        return new DeviceDto
        {
            Id = device.Id,
            Name = device.Name,
            Description = device.Description,
            DataState = device.DataState,
            OperationalState = device.OperationalState,
            CreatedAt = device.CreatedAt,
            ModifiedAt = device.ModifiedAt,
            CategoryId = device.CategoryId,
            DeviceCardId = device.DeviceCardId, // Keep this in DTO for reference
            Properties = device.Properties,
            OrderIndex = device.OrderIndex,
            IsCompleted = device.IsCompleted
        };
    }
    
    private bool IsValidJson(string? jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString)) return true;
        try
        {
            JsonDocument.Parse(jsonString);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
} 