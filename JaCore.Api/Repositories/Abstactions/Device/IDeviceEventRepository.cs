using JaCore.Api.Entities.Device; // Assuming entities exist for events
// using JaCore.Api.DTOs.Device; // DTOs likely not needed directly in Repository interface
using System.Linq.Expressions;

namespace JaCore.Api.Repositories.Abstactions.Device;

// Basic Repository Interface for Device Events
public interface IDeviceEventRepository
{
    Task<DeviceEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DeviceEvent>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<DeviceEvent>> FindAsync(Expression<Func<DeviceEvent, bool>> predicate, CancellationToken cancellationToken = default); // Adding FindAsync
    Task<IEnumerable<DeviceEvent>> GetByDeviceCardIdAsync(Guid deviceCardId, CancellationToken cancellationToken = default);
    Task AddAsync(DeviceEvent deviceEvent, CancellationToken cancellationToken = default);
    // No UpdateAsync typically for events
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    IQueryable<DeviceEvent> GetAllQueryable(); // Add Queryable
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default); // Common pattern

    // Remove method often expected by services, even if not async
    void Remove(DeviceEvent deviceEvent);

    // Update method often expected by services (though maybe not used for events)
    void Update(DeviceEvent deviceEvent);
} 