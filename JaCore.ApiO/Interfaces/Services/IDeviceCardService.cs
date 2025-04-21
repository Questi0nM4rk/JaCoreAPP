using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;

namespace JaCore.Api.Interfaces.Services;

/// <summary>
/// Interface for the service managing DeviceCard entities.
/// </summary>
public interface IDeviceCardService
{
    /// <summary>
    /// Gets a device card by its associated device ID asynchronously.
    /// </summary>
    /// <param name="deviceId">The ID of the associated device.</param>
    /// <returns>The DeviceCard DTO, or null if not found.</returns>
    Task<DeviceCardDto?> GetByDeviceIdAsync(int deviceId);
    
    /// <summary>
    /// Gets a device card by its own ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the device card.</param>
    /// <returns>The DeviceCard DTO, or null if not found.</returns>
    Task<DeviceCardDto?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new device card for a given device asynchronously.
    /// </summary>
    /// <param name="deviceId">The ID of the device to associate the card with.</param>
    /// <param name="createDto">The DTO containing data for the new device card.</param>
    /// <returns>The created DeviceCard DTO.</returns>
    /// <exception cref="ArgumentException">Thrown if the device ID is invalid or a card already exists for the device.</exception>
    Task<DeviceCardDto> CreateAsync(int deviceId, CreateDeviceCardDto createDto);

    /// <summary>
    /// Updates an existing device card asynchronously.
    /// </summary>
    /// <param name="id">The ID of the device card to update.</param>
    /// <param name="updateDto">The DTO containing the updated data.</param>
    /// <returns>True if the update was successful, false if the device card was not found.</returns>
    /// <exception cref="ArgumentException">Thrown on validation errors.</exception>
    Task<bool> UpdateAsync(int id, UpdateDeviceCardDto updateDto);

    /// <summary>
    /// Deletes a device card asynchronously.
    /// </summary>
    /// <param name="id">The ID of the device card to delete.</param>
    /// <returns>True if the deletion was successful, false if the device card was not found.</returns>
    Task<bool> DeleteAsync(int id);
} 