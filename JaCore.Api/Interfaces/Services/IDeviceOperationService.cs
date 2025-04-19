using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;
using System.Threading.Tasks;

namespace JaCore.Api.Interfaces.Services;

/// <summary>
/// Interface for the service managing DeviceOperation entities.
/// </summary>
public interface IDeviceOperationService
{
    // Removed old GetAllAsync that wasn't context-specific
    // Task<PaginatedListDto<DeviceOperationDto>> GetAllAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Gets all operations associated with a specific DeviceCard asynchronously.
    /// </summary>
    /// <param name="deviceCardId">The ID of the device card.</param>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>A paginated list of DeviceOperation DTOs.</returns>
    Task<PaginatedListDto<DeviceOperationDto>> GetByDeviceCardIdAsync(int deviceCardId, int pageNumber, int pageSize);

    /// <summary>
    /// Gets a specific device operation by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the device operation.</param>
    /// <returns>The DeviceOperation DTO, or null if not found.</returns>
    Task<DeviceOperationDto?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new device operation asynchronously.
    /// Note: The DeviceCardId should be part of the CreateDeviceOperationDto.
    /// </summary>
    /// <param name="createDto">The DTO containing data for the new device operation.</param>
    /// <returns>The created DeviceOperation DTO.</returns>
    /// <exception cref="ArgumentException">Thrown if referenced DeviceCardId doesn't exist or validation fails.</exception>
    Task<DeviceOperationDto> CreateAsync(CreateDeviceOperationDto createDto);

    /// <summary>
    /// Updates an existing device operation asynchronously.
    /// </summary>
    /// <param name="id">The ID of the device operation to update.</param>
    /// <param name="updateDto">The DTO containing the updated data.</param>
    /// <returns>True if the update was successful, false if the device operation was not found.</returns>
    /// <exception cref="ArgumentException">Thrown on validation errors.</exception>
    Task<bool> UpdateAsync(int id, UpdateDeviceOperationDto updateDto);

    /// <summary>
    /// Deletes a device operation asynchronously.
    /// </summary>
    /// <param name="id">The ID of the device operation to delete.</param>
    /// <returns>True if the deletion was successful, false if the device operation was not found.</returns>
    Task<bool> DeleteAsync(int id);
    
    // Potentially add methods like:
    // Task<bool> SetCompletionStatusAsync(int id, bool isCompleted);
} 