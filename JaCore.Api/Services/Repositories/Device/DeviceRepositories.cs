using JaCore.Api.Data;
using JaCore.Api.Entities.Device;
using Microsoft.EntityFrameworkCore;

namespace JaCore.Api.Services.Repositories.Device;

// Specific interfaces inheriting from the generic one
// Add entity-specific methods here if needed beyond generic CRUD

public interface IDeviceRepository : IGenericRepository<Entities.Device.Device>
{
    Task<Entities.Device.Device?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default);
}

public interface IDeviceCardRepository : IGenericRepository<DeviceCard> { /* Specific methods for DeviceCard */ }

public interface ICategoryRepository : IGenericRepository<Category> { /* Specific methods for Category */ }

public interface ISupplierRepository : IGenericRepository<Supplier> { /* Specific methods for Supplier */ }

public interface IServiceRepository : IGenericRepository<Entities.Device.Service> { /* Specific methods for Service */ }

public interface IEventRepository : IGenericRepository<Event> { /* Specific methods for Event */ }

public interface IDeviceOperationRepository : IGenericRepository<DeviceOperation> { /* Specific methods for DeviceOperation */ }


// Specific implementations inheriting from the generic one

public class DeviceRepository : GenericRepository<Entities.Device.Device>, IDeviceRepository
{
    public DeviceRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Entities.Device.Device?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(d => d.SerialNumber == serialNumber, cancellationToken);
    }
    // Implement other specific methods here
}

public class DeviceCardRepository : GenericRepository<DeviceCard>, IDeviceCardRepository
{
    public DeviceCardRepository(ApplicationDbContext context) : base(context) { }
    // Implement specific methods here
}

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context) { }
    // Implement specific methods here
}

public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
{
    public SupplierRepository(ApplicationDbContext context) : base(context) { }
    // Implement specific methods here
}

public class ServiceRepository : GenericRepository<Entities.Device.Service>, IServiceRepository
{
    public ServiceRepository(ApplicationDbContext context) : base(context) { }
    // Implement specific methods here
}

public class EventRepository : GenericRepository<Event>, IEventRepository
{
    public EventRepository(ApplicationDbContext context) : base(context) { }
    // Implement specific methods here
}

public class DeviceOperationRepository : GenericRepository<DeviceOperation>, IDeviceOperationRepository
{
    public DeviceOperationRepository(ApplicationDbContext context) : base(context) { }
    // Implement specific methods here
} 