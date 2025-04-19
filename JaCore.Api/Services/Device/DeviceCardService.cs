using AutoMapper;
using JaCore.Api.Data; // May need for validation checks
using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Interfaces.Services;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Models.Device;
using Microsoft.EntityFrameworkCore; // For DbUpdateConcurrencyException and AnyAsync
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace JaCore.Api.Services.Device;

/// <summary>
/// Service implementation for managing DeviceCard entities.
/// </summary>
public class DeviceCardService : IDeviceCardService
{
    private readonly IDeviceCardRepository _cardRepository;
    private readonly IDeviceRepository _deviceRepository; // Needed to link card to device
    private readonly ILogger<DeviceCardService> _logger;

    public DeviceCardService(
        IDeviceCardRepository cardRepository, 
        IDeviceRepository deviceRepository, // Inject device repo
        ILogger<DeviceCardService> logger)
    {
        _cardRepository = cardRepository;
        _deviceRepository = deviceRepository;
        _logger = logger;
    }

    public async Task<DeviceCardDto?> GetByDeviceIdAsync(int deviceId)
    {
        var card = await _cardRepository.GetByDeviceIdAsync(deviceId);
        return card == null ? null : MapToDto(card);
    }

    public async Task<DeviceCardDto?> GetByIdAsync(int id)
    {
        var card = await _cardRepository.GetByIdAsync(id);
        return card == null ? null : MapToDto(card);
    }

    public async Task<DeviceCardDto> CreateAsync(int deviceId, CreateDeviceCardDto createDto)
    {
        _logger.LogInformation("Attempting to create DeviceCard for DeviceId: {DeviceId}", deviceId);

        // 1. Validate Device exists
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        if (device == null)
        {
            _logger.LogWarning("Device with Id {DeviceId} not found for creating DeviceCard.", deviceId);
            throw new ArgumentException($"Device with Id {deviceId} not found.", nameof(deviceId));
        }

        // 2. Check if device already has a card
        if (device.DeviceCardId.HasValue)
        {
             _logger.LogWarning("Device {DeviceId} already has a DeviceCard (Id: {DeviceCardId}). Cannot create another.", deviceId, device.DeviceCardId.Value);
             throw new InvalidOperationException($"Device {deviceId} already has an associated DeviceCard.");
        }
        
        // 3. TODO: Add validation for createDto (e.g., SupplierId/ServiceId exist if provided)
        if (!string.IsNullOrWhiteSpace(createDto.SerialNumber) && createDto.SerialNumber.Length > 100) // Example
        {
             throw new ArgumentException("SerialNumber cannot exceed 100 characters.", nameof(createDto.SerialNumber));
        }

        var newCard = new DeviceCard
        {
            // Don't set DeviceId directly if EF manages the relationship via navigation property
            SerialNumber = createDto.SerialNumber,
            DateOfActivation = createDto.DateOfActivation,
            SupplierId = createDto.SupplierId,
            ServiceId = createDto.ServiceId,
            MetConLevel1 = createDto.MetConLevel1,
            MetConLevel2 = createDto.MetConLevel2,
            MetConLevel3 = createDto.MetConLevel3,
            MetConLevel4 = createDto.MetConLevel4,
            // Link back to the device - important for the relationship
            Device = device 
        };

        try
        {
            var addedCard = await _cardRepository.AddAsync(newCard);
            
            // 4. Update the Device entity with the new DeviceCardId
            device.DeviceCardId = addedCard.Id;
            await _deviceRepository.UpdateAsync(device); 
            
            _logger.LogInformation("Successfully created DeviceCard {DeviceCardId} for DeviceId: {DeviceId}", addedCard.Id, deviceId);
            return MapToDto(addedCard);
        }
        catch (Exception ex)
        {           
            _logger.LogError(ex, "Error occurred while creating DeviceCard for DeviceId: {DeviceId}", deviceId);
            // Consider cleanup if card was added but device update failed?
            throw;
        }
    }

    public async Task<bool> UpdateAsync(int id, UpdateDeviceCardDto updateDto)
    {
        _logger.LogInformation("Attempting to update DeviceCard with Id: {DeviceCardId}", id);
        var existingCard = await _cardRepository.GetByIdAsync(id);
        if (existingCard == null)
        {
            _logger.LogWarning("DeviceCard with Id {DeviceCardId} not found for update.", id);
            return false;
        }

        // TODO: Add validation for updateDto (e.g., SupplierId/ServiceId exist if provided)
        if (!string.IsNullOrWhiteSpace(updateDto.SerialNumber) && updateDto.SerialNumber.Length > 100) // Example
        {
             throw new ArgumentException("SerialNumber cannot exceed 100 characters.", nameof(updateDto.SerialNumber));
        }

        // Map fields
        existingCard.SerialNumber = updateDto.SerialNumber;
        existingCard.DateOfActivation = updateDto.DateOfActivation;
        existingCard.SupplierId = updateDto.SupplierId;
        existingCard.ServiceId = updateDto.ServiceId;
        existingCard.MetConLevel1 = updateDto.MetConLevel1;
        existingCard.MetConLevel2 = updateDto.MetConLevel2;
        existingCard.MetConLevel3 = updateDto.MetConLevel3;
        existingCard.MetConLevel4 = updateDto.MetConLevel4;
        // Don't update Device relationship here

        try
        {
            await _cardRepository.UpdateAsync(existingCard);
             _logger.LogInformation("Successfully updated DeviceCard {DeviceCardId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating DeviceCard {DeviceCardId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
         _logger.LogInformation("Attempting to delete DeviceCard with Id: {DeviceCardId}", id);
        var existingCard = await _cardRepository.GetByIdAsync(id);
        if (existingCard == null)
        {
            _logger.LogWarning("DeviceCard with Id {DeviceCardId} not found for deletion.", id);
            return false;
        }
        
        // Important: Unlink from Device before deleting card
        var device = await _deviceRepository.GetByDeviceCardIdAsync(id); // Need this method in IDeviceRepository
        if (device != null) 
        { 
            device.DeviceCardId = null;
            await _deviceRepository.UpdateAsync(device);
            _logger.LogInformation("Unlinked Device {DeviceId} from DeviceCard {DeviceCardId}", device.Id, id);
        } 
        else 
        { 
             _logger.LogWarning("Could not find associated Device for DeviceCard {DeviceCardId} during delete.", id);
             // Decide if this is an error or just a warning
        }

        try
        {
            await _cardRepository.DeleteAsync(id); // Assuming DeleteAsync takes Id
            _logger.LogInformation("Successfully deleted DeviceCard {DeviceCardId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting DeviceCard {DeviceCardId}", id);
            // Consider rolling back device update if delete fails?
            throw;
        }
    }

    // Simple mapper (Consider AutoMapper)
    private static DeviceCardDto MapToDto(DeviceCard card)
    {
        return new DeviceCardDto
        {
            Id = card.Id,
            SerialNumber = card.SerialNumber,
            DateOfActivation = card.DateOfActivation,
            SupplierId = card.SupplierId,
            ServiceId = card.ServiceId,
            MetConLevel1 = card.MetConLevel1,
            MetConLevel2 = card.MetConLevel2,
            MetConLevel3 = card.MetConLevel3,
            MetConLevel4 = card.MetConLevel4,
            // DeviceId = card.Device?.Id // Need Device navigation property loaded
        };
    }
}

// Extension method needed for IDeviceRepository
public static class DeviceRepositoryExtensions
{
    public static async Task<Models.Device.Device?> GetByDeviceCardIdAsync(this IDeviceRepository repo, int deviceCardId)
    {
        // This relies on the underlying implementation detail (likely EF)
        // A cleaner approach might involve a dedicated method in the interface/implementation
        var devices = await repo.GetAllAsync(1, 1, d => d.DeviceCardId == deviceCardId); 
        return devices.FirstOrDefault();
    }
} 