using JaCore.Api.Interfaces.Common;
using JaCore.Api.Models.Device;
using JaCore.Api.Dtos.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JaCore.Api.Interfaces.Repositories.Device;

/// <summary>
/// Interface for the repository managing Event entities.
/// </summary>
public interface IEventRepository : IRepository<Event>
{
    /// <summary>
    /// Gets all events associated with a specific DeviceCard asynchronously.
    /// </summary>
    /// <param name="deviceCardId">The ID of the device card.</param>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>A paginated list of Event entities.</returns>
    Task<JaCore.Api.Dtos.Common.PaginatedListDto<Event>> GetByDeviceCardIdAsync(int deviceCardId, int pageNumber, int pageSize);

    // Add other Event-specific methods if needed
} 