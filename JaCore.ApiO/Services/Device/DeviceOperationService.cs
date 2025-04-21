using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Interfaces.Services;
using JaCore.Api.Models.Device;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace JaCore.Api.Services.Device;

/// <summary>
/// Service implementation for managing DeviceOperation entities.
/// </summary>
public class DeviceOperationService : IDeviceOperationService
{
    private readonly IDeviceOperationRepository _operationRepository;
    private readonly IDeviceCardRepository _cardRepository; // Needed for validation
    private readonly ILogger<DeviceOperationService> _logger;

    public DeviceOperationService(
        IDeviceOperationRepository operationRepository,
        IDeviceCardRepository cardRepository, // Inject card repo
        ILogger<DeviceOperationService> logger)
    {
        _operationRepository = operationRepository;
        _cardRepository = cardRepository;
        _logger = logger;
    }

    public async Task<PaginatedListDto<DeviceOperationDto>> GetByDeviceCardIdAsync(int deviceCardId, int pageNumber, int pageSize)
    {
        _logger.LogInformation("Getting DeviceOperations for DeviceCardId: {DeviceCardId}", deviceCardId);
        
        // Validate DeviceCard exists
        var cardExists = await _cardRepository.ExistsAsync(deviceCardId); 
        if (!cardExists)
        {
            // Or return empty list? Throwing allows controller to return 404 maybe?
            _logger.LogWarning("DeviceCard with Id {DeviceCardId} not found when getting operations.", deviceCardId);
            // Depending on desired behavior, could return an empty list or throw.
            // Returning empty list for now.
            return new PaginatedListDto<DeviceOperationDto>(Enumerable.Empty<DeviceOperationDto>(), 0, pageNumber, pageSize);
            // throw new ArgumentException($"DeviceCard with Id {deviceCardId} not found.", nameof(deviceCardId));
        }
        
        var paginatedOperations = await _operationRepository.GetByDeviceCardIdAsync(deviceCardId, pageNumber, pageSize);
        
        var operationDtos = paginatedOperations.Items.Select(op => MapToDto(op));
        
        return new PaginatedListDto<DeviceOperationDto>(operationDtos, paginatedOperations.TotalCount, pageNumber, pageSize);
    }

    public async Task<DeviceOperationDto?> GetByIdAsync(int id)
    {
        var operation = await _operationRepository.GetByIdAsync(id);
        return operation == null ? null : MapToDto(operation);
    }

    public async Task<DeviceOperationDto> CreateAsync(CreateDeviceOperationDto createDto)
    {
        _logger.LogInformation("Attempting to create DeviceOperation for DeviceCardId: {DeviceCardId}", createDto.DeviceCardId);
        
        // 1. Validate DeviceCard exists
        var cardExists = await _cardRepository.ExistsAsync(createDto.DeviceCardId);
        if (!cardExists)
        {
             _logger.LogWarning("DeviceCard with Id {DeviceCardId} not found for creating DeviceOperation.", createDto.DeviceCardId);
            throw new ArgumentException($"DeviceCard with Id {createDto.DeviceCardId} not found.", nameof(createDto.DeviceCardId));
        }
        
        // 2. Validate DTO fields
        if (string.IsNullOrWhiteSpace(createDto.Name) || createDto.Name.Length > 100)
        {
            throw new ArgumentException("Operation name is required and cannot exceed 100 characters.", nameof(createDto.Name));
        }
        if (!string.IsNullOrEmpty(createDto.UiElements) && !IsValidJson(createDto.UiElements))
        {
            throw new ArgumentException("UiElements must be valid JSON.", nameof(createDto.UiElements));
        }

        var newOperation = new DeviceOperation
        {
            Name = createDto.Name,
            Description = createDto.Description,
            OrderIndex = createDto.OrderIndex,
            DeviceCardId = createDto.DeviceCardId, // Set the FK
            IsRequired = createDto.IsRequired,
            IsCompleted = createDto.IsCompleted, // Default to false on create?
            UiElements = createDto.UiElements
        };

        try
        {
            var addedOperation = await _operationRepository.AddAsync(newOperation);
            _logger.LogInformation("Successfully created DeviceOperation {DeviceOperationId} for DeviceCardId: {DeviceCardId}", addedOperation.Id, addedOperation.DeviceCardId);
            return MapToDto(addedOperation);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error occurred while creating DeviceOperation for DeviceCardId: {DeviceCardId}", createDto.DeviceCardId);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(int id, UpdateDeviceOperationDto updateDto)
    {
        _logger.LogInformation("Attempting to update DeviceOperation with Id: {DeviceOperationId}", id);
        var existingOperation = await _operationRepository.GetByIdAsync(id);
        if (existingOperation == null)
        {
             _logger.LogWarning("DeviceOperation with Id {DeviceOperationId} not found for update.", id);
            return false;
        }
        
        // Validate DTO fields
        if (string.IsNullOrWhiteSpace(updateDto.Name) || updateDto.Name.Length > 100)
        {
            throw new ArgumentException("Operation name is required and cannot exceed 100 characters.", nameof(updateDto.Name));
        }
        if (!string.IsNullOrEmpty(updateDto.UiElements) && !IsValidJson(updateDto.UiElements))
        {
            throw new ArgumentException("UiElements must be valid JSON.", nameof(updateDto.UiElements));
        }
        
        // Map fields (don't update DeviceCardId)
        existingOperation.Name = updateDto.Name;
        existingOperation.Description = updateDto.Description;
        existingOperation.OrderIndex = updateDto.OrderIndex;
        existingOperation.IsRequired = updateDto.IsRequired;
        existingOperation.IsCompleted = updateDto.IsCompleted;
        existingOperation.UiElements = updateDto.UiElements;
        
        try
        {
            await _operationRepository.UpdateAsync(existingOperation);
            _logger.LogInformation("Successfully updated DeviceOperation {DeviceOperationId}", id);
            return true;
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error occurred while updating DeviceOperation {DeviceOperationId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Attempting to delete DeviceOperation with Id: {DeviceOperationId}", id);
        var existingOperation = await _operationRepository.GetByIdAsync(id);
        if (existingOperation == null)
        {
             _logger.LogWarning("DeviceOperation with Id {DeviceOperationId} not found for deletion.", id);
            return false;
        }

        try
        {
            await _operationRepository.DeleteAsync(id);
            _logger.LogInformation("Successfully deleted DeviceOperation {DeviceOperationId}", id);
            return true;
        }
        catch (Exception ex)
        {           
            _logger.LogError(ex, "Error occurred while deleting DeviceOperation {DeviceOperationId}", id);
            throw;
        }
    }
    
    // --- Helper Methods ---
    
    private static DeviceOperationDto MapToDto(DeviceOperation operation)
    {
        return new DeviceOperationDto
        {
            Id = operation.Id,
            Name = operation.Name,
            Description = operation.Description,
            OrderIndex = operation.OrderIndex,
            DeviceCardId = operation.DeviceCardId,
            IsRequired = operation.IsRequired,
            IsCompleted = operation.IsCompleted,
            UiElements = operation.UiElements
        };
    }
    
    private bool IsValidJson(string? jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString)) return true; // Allow null/empty
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