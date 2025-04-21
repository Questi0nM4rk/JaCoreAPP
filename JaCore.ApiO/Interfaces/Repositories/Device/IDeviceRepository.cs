using JaCore.Api.Interfaces.Common;
using JaCore.Api.Models.Device;

namespace JaCore.Api.Interfaces.Repositories.Device;

/// <summary>
/// Interface for the repository managing Device entities.
/// </summary>
public interface IDeviceRepository : IRepository<JaCore.Api.Models.Device.Device>
{
    // Add specific methods if needed, e.g.,
    // Task<IEnumerable<Device>> GetDevicesWithExpiringWarrantyAsync(DateTime threshold);
    
    /// <summary>
    /// Gets a device by its associated DeviceCard ID asynchronously.
    /// </summary>
    /// <param name="deviceCardId">The ID of the associated device card.</param>
    /// <returns>The Device entity, or null if no device is linked to this card ID.</returns>
    Task<JaCore.Api.Models.Device.Device?> GetByDeviceCardIdAsync(int deviceCardId);
} 