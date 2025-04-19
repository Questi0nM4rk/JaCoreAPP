using JaCore.Api.Interfaces.Common;
using JaCore.Api.Models.Device;

namespace JaCore.Api.Interfaces.Repositories.Device;

/// <summary>
/// Interface for the repository managing DeviceCard entities.
/// Extends the generic repository but can include specific methods if needed.
/// </summary>
public interface IDeviceCardRepository : IRepository<DeviceCard>
{
    /// <summary>
    /// Gets a device card by its associated device ID asynchronously.
    /// </summary>
    /// <param name="deviceId">The ID of the associated device.</param>
    /// <returns>The DeviceCard entity, or null if not found.</returns>
    Task<DeviceCard?> GetByDeviceIdAsync(int deviceId);
    
    // Add other DeviceCard-specific methods here if needed
    // e.g., Task<bool> DoesSerialNumberExistAsync(string serialNumber);
} 