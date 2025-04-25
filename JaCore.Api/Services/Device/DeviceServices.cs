using JaCore.Api.Data;
using JaCore.Api.DTOs.Device;
using JaCore.Api.Services.Repositories.Device;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace JaCore.Api.Services.Device;

// Base Service (Optional) - Could contain common logic like logging, Unit of Work pattern

public abstract class BaseDeviceService
{
    protected readonly ILogger _logger;
    protected readonly ApplicationDbContext _context; // For SaveChangesAsync

    protected BaseDeviceService(ILogger logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    protected async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to the database.");
            return false;
        }
    }

    // Add common validation or mapping logic here if desired
}

// Implementations for each service

public class DeviceService : BaseDeviceService, IDeviceService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISupplierRepository _supplierRepository;

    public DeviceService(
        IDeviceRepository deviceRepository,
        ICategoryRepository categoryRepository,
        ISupplierRepository supplierRepository,
        ApplicationDbContext context,
        ILogger<DeviceService> logger) : base(logger, context)
    {
        _deviceRepository = deviceRepository;
        _categoryRepository = categoryRepository;
        _supplierRepository = supplierRepository;
    }

    public async Task<DeviceReadDto?> CreateDeviceAsync(DeviceCreateDto createDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new device: {DeviceName}", createDto.Name);
        try
        {
            // Basic Validation Example (more complex validation would go here)
            if (await _deviceRepository.ExistsAsync(d => d.SerialNumber == createDto.SerialNumber, cancellationToken))
            {
                _logger.LogWarning("Device creation failed: Serial number {SerialNumber} already exists.", createDto.SerialNumber);
                return null; // Or return a Result object indicating failure
            }

            var device = new Entities.Device.Device
            {
                Name = createDto.Name,
                SerialNumber = createDto.SerialNumber,
                ModelNumber = createDto.ModelNumber,
                Manufacturer = createDto.Manufacturer,
                PurchaseDate = createDto.PurchaseDate,
                WarrantyExpiryDate = createDto.WarrantyExpiryDate,
                CategoryId = createDto.CategoryId,
                SupplierId = createDto.SupplierId
            };

            await _deviceRepository.AddAsync(device, cancellationToken);
            bool saved = await SaveChangesAsync(cancellationToken);

            if (!saved)
            {
                _logger.LogError("Failed to save new device {DeviceName}", createDto.Name);
                return null;
            }

            _logger.LogInformation("Successfully created device {DeviceName} with ID {DeviceId}", device.Name, device.Id);
            // Refetch or map to include Category/Supplier names if needed immediately
            return await GetDeviceByIdAsync(device.Id, cancellationToken); // Simple refetch for now
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error during device creation process for {DeviceName}", createDto.Name);
            return null;
        }
    }

    public async Task<bool> DeleteDeviceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to delete device with ID: {DeviceId}", id);
        try
        {
            var device = await _deviceRepository.GetByIdAsync(id, cancellationToken);
            if (device == null)
            {
                _logger.LogWarning("Device deletion failed: Device with ID {DeviceId} not found.", id);
                return false;
            }

            _deviceRepository.Remove(device);
            bool saved = await SaveChangesAsync(cancellationToken);

            if (!saved)
            {
                _logger.LogError("Failed to save deletion for device {DeviceId}", id);
            }
            else
            {
                _logger.LogInformation("Successfully deleted device {DeviceId}", id);
            }
            return saved;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error during device deletion process for ID {DeviceId}", id);
            return false;
        }
    }

    public async Task<IEnumerable<DeviceReadDto>> GetAllDevicesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching all devices");
        try
        {
            var devices = await _deviceRepository.GetAllAsync(cancellationToken, d => d.Category!, d => d.Supplier!); // Include related data

            // Simple Mapping (Consider AutoMapper or Mapster for complex scenarios)
            return devices.Select(d => new DeviceReadDto(
                d.Id,
                d.Name,
                d.SerialNumber,
                d.ModelNumber,
                d.Manufacturer,
                d.PurchaseDate,
                d.WarrantyExpiryDate,
                d.CategoryId,
                d.Category?.Name,
                d.SupplierId,
                d.Supplier?.Name,
                d.CreatedAt,
                d.ModifiedAt
            ));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving all devices");
            return Enumerable.Empty<DeviceReadDto>();
        }
    }

    public async Task<DeviceReadDto?> GetDeviceByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching device by ID: {DeviceId}", id);
        try
        {
            var device = await _deviceRepository.GetByIdAsync(id, cancellationToken, d => d.Category!, d => d.Supplier!); // Include related data

            if (device == null) return null;

            return new DeviceReadDto(
                device.Id,
                device.Name,
                device.SerialNumber,
                device.ModelNumber,
                device.Manufacturer,
                device.PurchaseDate,
                device.WarrantyExpiryDate,
                device.CategoryId,
                device.Category?.Name,
                device.SupplierId,
                device.Supplier?.Name,
                device.CreatedAt,
                device.ModifiedAt
            );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving device with ID {DeviceId}", id);
            return null;
        }
    }

    public async Task<DeviceReadDto?> GetDeviceBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default)
    {
         _logger.LogDebug("Fetching device by Serial Number: {SerialNumber}", serialNumber);
         try
         {
             // Need to implement GetBySerialNumberAsync in the repository or use FindAsync
             var devices = await _deviceRepository.FindAsync(d => d.SerialNumber == serialNumber, cancellationToken, d => d.Category!, d => d.Supplier!); // Include related data
             var singleDevice = devices.FirstOrDefault();

             if (singleDevice == null) return null;

             return new DeviceReadDto(
                 singleDevice.Id,
                 singleDevice.Name,
                 singleDevice.SerialNumber,
                 singleDevice.ModelNumber,
                 singleDevice.Manufacturer,
                 singleDevice.PurchaseDate,
                 singleDevice.WarrantyExpiryDate,
                 singleDevice.CategoryId,
                 singleDevice.Category?.Name,
                 singleDevice.SupplierId,
                 singleDevice.Supplier?.Name,
                 singleDevice.CreatedAt,
                 singleDevice.ModifiedAt
             );
         }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
             _logger.LogError(ex, "Error retrieving device with serial number {SerialNumber}", serialNumber);
             return null;
        }
    }

    public async Task<bool> UpdateDeviceAsync(Guid id, DeviceUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to update device with ID: {DeviceId}", id);
        try 
        {
            var device = await _deviceRepository.GetByIdAsync(id, cancellationToken);
            if (device == null)
            {
                _logger.LogWarning("Device update failed: Device with ID {DeviceId} not found.", id);
                return false;
            }

            // Update properties (Consider AutoMapper or Mapster)
            device.Name = updateDto.Name;
            device.ModelNumber = updateDto.ModelNumber;
            device.Manufacturer = updateDto.Manufacturer;
            device.PurchaseDate = updateDto.PurchaseDate;
            device.WarrantyExpiryDate = updateDto.WarrantyExpiryDate;
            device.CategoryId = updateDto.CategoryId;
            device.SupplierId = updateDto.SupplierId;
            // ModifiedAt will be updated by BaseEntity handling or interceptor

            _deviceRepository.Update(device);
            bool saved = await SaveChangesAsync(cancellationToken);

            if (!saved)
            {
                 _logger.LogError("Failed to save update for device {DeviceId}", id);
            }
            else
            {
                _logger.LogInformation("Successfully updated device {DeviceId}", id);
            }
            return saved;
        }
        catch (DbUpdateConcurrencyException) // Specific catch for concurrency - remove unused 'ex'
        {
            // Base service already logs Error on SaveChanges failure, just return false.
            // _logger.LogWarning(ex, "Concurrency issue while updating device {DeviceId}.", id);
            return false;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error during device update process for ID {DeviceId}", id);
            return false;
        }
    }

    // Implement other methods...
}

// --- Placeholder Implementations for other services --- 

public class DeviceCardService : BaseDeviceService, IDeviceCardService
{
    private readonly IDeviceCardRepository _repository;
    private readonly IDeviceRepository _deviceRepository; // Need to check if Device exists

    public DeviceCardService(IDeviceCardRepository repository, IDeviceRepository deviceRepository, ApplicationDbContext context, ILogger<DeviceCardService> logger)
        : base(logger, context)
    {
        _repository = repository;
        _deviceRepository = deviceRepository;
    }

    public async Task<DeviceCardReadDto?> CreateDeviceCardAsync(DeviceCardCreateDto createDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to create DeviceCard for Device ID: {DeviceId}", createDto.DeviceId);
        try
        {
            // Validate Device exists
            if (!await _deviceRepository.ExistsAsync(createDto.DeviceId, cancellationToken))
            {
                _logger.LogWarning("DeviceCard creation failed: Device with ID {DeviceId} not found.", createDto.DeviceId);
                return null;
            }

            // Validate DeviceCard doesn't already exist for the device
            if (await _repository.ExistsAsync(c => c.DeviceId == createDto.DeviceId, cancellationToken))
            {
                _logger.LogWarning("DeviceCard creation failed: A card already exists for Device ID {DeviceId}.", createDto.DeviceId);
                return null;
            }

            var card = new Entities.Device.DeviceCard
            {
                DeviceId = createDto.DeviceId,
                Location = createDto.Location,
                AssignedUser = createDto.AssignedUser,
                PropertiesJson = createDto.PropertiesJson,
                // Set initial states/timestamps if needed
                LastSeenAt = DateTimeOffset.UtcNow // Example: set LastSeen on creation
            };

            await _repository.AddAsync(card, cancellationToken);
            bool saved = await SaveChangesAsync(cancellationToken);

            if (!saved)
            {
                _logger.LogError("Failed to save new DeviceCard for Device ID {DeviceId}", createDto.DeviceId);
                return null;
            }

            _logger.LogInformation("Successfully created DeviceCard {CardId} for Device ID {DeviceId}", card.Id, card.DeviceId);
            return MapToReadDto(card); // Use mapping function
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error during DeviceCard creation for Device ID {DeviceId}", createDto.DeviceId);
            return null;
        }
    }

    public async Task<bool> DeleteDeviceCardAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting DeviceCard with ID: {CardId}", id);
        try
        {
            var card = await _repository.GetByIdAsync(id, cancellationToken);
            if (card == null)
            {
                _logger.LogWarning("DeviceCard deletion failed: Card not found with ID {CardId}.", id);
                return false;
            }

            _repository.Remove(card);
            bool saved = await SaveChangesAsync(cancellationToken);

            if (!saved)
            {
                _logger.LogError("Failed to save deletion for DeviceCard {CardId}", id);
            }
            else 
            {
                 _logger.LogInformation("Successfully deleted DeviceCard {CardId}", id);
            }
            return saved;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error during DeviceCard deletion process for ID {CardId}", id);
            return false;
        }
    }

    public async Task<IEnumerable<DeviceCardReadDto>> GetAllDeviceCardsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching all DeviceCards.");
        try
        {
            var cards = await _repository.GetAllAsync(cancellationToken);
            return cards.Select(MapToReadDto);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving all DeviceCards.");
            return Enumerable.Empty<DeviceCardReadDto>();
        }
    }

    public async Task<DeviceCardReadDto?> GetDeviceCardByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching DeviceCard by ID: {CardId}", id);
        try
        {
            var card = await _repository.GetByIdAsync(id, cancellationToken);
            return card == null ? null : MapToReadDto(card);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving device card with ID {CardId}", id);
            return null;
        }
    }

    public async Task<IEnumerable<DeviceCardReadDto>> GetDeviceCardsByDeviceIdAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching DeviceCards for Device ID: {DeviceId}", deviceId);
        try
        {
            var cards = await _repository.FindAsync(c => c.DeviceId == deviceId, cancellationToken);
            return cards.Select(MapToReadDto);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving device cards for device ID {DeviceId}", deviceId);
            return Enumerable.Empty<DeviceCardReadDto>();
        }
    }

    public async Task<bool> UpdateDeviceCardAsync(Guid id, DeviceCardUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating DeviceCard with ID: {CardId}", id);
        try
        {
            var card = await _repository.GetByIdAsync(id, cancellationToken);
            if (card == null)
            {
                _logger.LogWarning("DeviceCard update failed: Card not found with ID {CardId}.", id);
                return false;
            }

            // Update properties
            card.Location = updateDto.Location;
            card.AssignedUser = updateDto.AssignedUser;
            card.PropertiesJson = updateDto.PropertiesJson;
            // LastSeenAt could potentially be updated here or via a separate mechanism

            _repository.Update(card);
            bool saved = await SaveChangesAsync(cancellationToken);

            if (!saved)
            {
                 _logger.LogError("Failed to save update for DeviceCard {CardId}", id);
            }
            else
            {
                _logger.LogInformation("Successfully updated DeviceCard {CardId}", id);
            }
            return saved;
        }
        catch (DbUpdateConcurrencyException) // Specific catch for concurrency - remove unused 'ex'
        {
            // Base service already logs Error on SaveChanges failure, just return false.
            // _logger.LogWarning(ex, "Concurrency issue while updating device card {CardId}.", id);
            return false;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error during DeviceCard update process for ID {CardId}", id);
            return false;
        }
    }

    // Simple mapping function (Consider AutoMapper/Mapster)
    private static DeviceCardReadDto MapToReadDto(Entities.Device.DeviceCard card)
    {
        return new DeviceCardReadDto(
            card.Id,
            card.DeviceId,
            card.Location,
            card.AssignedUser,
            card.LastSeenAt,
            card.DataState,
            card.OperationalState,
            card.PropertiesJson,
            card.CreatedAt,
            card.ModifiedAt
        );
    }
}

// *** Implementations for remaining services ***

public class SupplierService : BaseDeviceService, ISupplierService
{
    private readonly ISupplierRepository _repository;
    public SupplierService(ISupplierRepository repository, ApplicationDbContext context, ILogger<SupplierService> logger)
        : base(logger, context)
    {
        _repository = repository;
    }

    public async Task<SupplierReadDto?> CreateSupplierAsync(SupplierCreateDto createDto, CancellationToken cancellationToken = default)
    {
        // Add validation if needed (e.g., unique name)
        var entity = new Entities.Device.Supplier
        {
            Name = createDto.Name,
            ContactName = createDto.ContactName,
            Email = createDto.Email,
            Phone = createDto.Phone,
            Address = createDto.Address
        };
        await _repository.AddAsync(entity, cancellationToken);
        return await SaveChangesAsync(cancellationToken) ? MapToReadDto(entity) : null;
    }

    public async Task<bool> DeleteSupplierAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;
        _repository.Remove(entity);
        return await SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<SupplierReadDto>> GetAllSuppliersAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(MapToReadDto);
    }

    public async Task<SupplierReadDto?> GetSupplierByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToReadDto(entity);
    }

    public async Task<bool> UpdateSupplierAsync(Guid id, SupplierUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;
        // Add validation if needed (e.g., unique name check)
        entity.Name = updateDto.Name;
        entity.ContactName = updateDto.ContactName;
        entity.Email = updateDto.Email;
        entity.Phone = updateDto.Phone;
        entity.Address = updateDto.Address;
        _repository.Update(entity);
        return await SaveChangesAsync(cancellationToken);
    }

    private static SupplierReadDto MapToReadDto(Entities.Device.Supplier e) =>
        new(e.Id, e.Name, e.ContactName, e.Email, e.Phone, e.Address);
}

public class ServiceService : BaseDeviceService, IServiceService
{
    private readonly IServiceRepository _repository;
    public ServiceService(IServiceRepository repository, ApplicationDbContext context, ILogger<ServiceService> logger)
        : base(logger, context)
    {
        _repository = repository;
    }

    public async Task<ServiceReadDto?> CreateServiceAsync(ServiceCreateDto createDto, CancellationToken cancellationToken = default)
    {
        var entity = new Entities.Device.Service
        {
            Name = createDto.Name,
            Description = createDto.Description,
            ProviderName = createDto.ProviderName,
            ContactInfo = createDto.ContactInfo
        };
        await _repository.AddAsync(entity, cancellationToken);
        return await SaveChangesAsync(cancellationToken) ? MapToReadDto(entity) : null;
    }

    public async Task<bool> DeleteServiceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;
        _repository.Remove(entity);
        return await SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<ServiceReadDto>> GetAllServicesAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(MapToReadDto);
    }

    public async Task<ServiceReadDto?> GetServiceByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToReadDto(entity);
    }

    public async Task<bool> UpdateServiceAsync(Guid id, ServiceUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;
        entity.Name = updateDto.Name;
        entity.Description = updateDto.Description;
        entity.ProviderName = updateDto.ProviderName;
        entity.ContactInfo = updateDto.ContactInfo;
        _repository.Update(entity);
        return await SaveChangesAsync(cancellationToken);
    }

    private static ServiceReadDto MapToReadDto(Entities.Device.Service e) =>
        new(e.Id, e.Name, e.Description, e.ProviderName, e.ContactInfo);
}

public class EventService : BaseDeviceService, IEventService
{
    private readonly IEventRepository _repository;
    private readonly IDeviceCardRepository _deviceCardRepository; // To validate DeviceCardId

    public EventService(IEventRepository repository, IDeviceCardRepository deviceCardRepository, ApplicationDbContext context, ILogger<EventService> logger)
        : base(logger, context)
    {
        _repository = repository;
        _deviceCardRepository = deviceCardRepository;
    }

    public async Task<EventReadDto?> CreateEventAsync(EventCreateDto createDto, CancellationToken cancellationToken = default)
    {
        if (!await _deviceCardRepository.ExistsAsync(createDto.DeviceCardId, cancellationToken))
        {
            _logger.LogWarning("Event creation failed: DeviceCardId {DeviceCardId} not found.", createDto.DeviceCardId);
            return null;
        }
        var entity = new Entities.Device.Event
        {
            DeviceCardId = createDto.DeviceCardId,
            Type = createDto.Type,
            Timestamp = createDto.Timestamp,
            Description = createDto.Description,
            DetailsJson = createDto.DetailsJson,
            TriggeredBy = createDto.TriggeredBy
        };
        await _repository.AddAsync(entity, cancellationToken);
        return await SaveChangesAsync(cancellationToken) ? MapToReadDto(entity) : null;
    }

    public async Task<bool> DeleteEventAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;
        _repository.Remove(entity);
        return await SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<EventReadDto>> GetAllEventsAsync(CancellationToken cancellationToken = default)
    {
        // Consider adding filtering/pagination here if the list could grow large
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(MapToReadDto);
    }

    public async Task<IEnumerable<EventReadDto>> GetEventsByDeviceCardIdAsync(Guid deviceCardId, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.FindAsync(e => e.DeviceCardId == deviceCardId, cancellationToken);
        return entities.Select(MapToReadDto);
    }

    public async Task<EventReadDto?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToReadDto(entity);
    }

    private static EventReadDto MapToReadDto(Entities.Device.Event e) =>
        new(e.Id, e.DeviceCardId, e.Type, e.Timestamp, e.Description, e.DetailsJson, e.TriggeredBy, e.CreatedAt, e.ModifiedAt);
}

public class DeviceOperationService : BaseDeviceService, IDeviceOperationService
{
    private readonly IDeviceOperationRepository _repository;
    private readonly IDeviceCardRepository _deviceCardRepository; // To validate DeviceCardId

    public DeviceOperationService(IDeviceOperationRepository repository, IDeviceCardRepository deviceCardRepository, ApplicationDbContext context, ILogger<DeviceOperationService> logger)
        : base(logger, context)
    {
        _repository = repository;
        _deviceCardRepository = deviceCardRepository;
    }

    public async Task<DeviceOperationReadDto?> CreateOperationAsync(DeviceOperationCreateDto createDto, CancellationToken cancellationToken = default)
    {
        if (!await _deviceCardRepository.ExistsAsync(createDto.DeviceCardId, cancellationToken))
        {
             _logger.LogWarning("Operation creation failed: DeviceCardId {DeviceCardId} not found.", createDto.DeviceCardId);
            return null;
        }
        var entity = new Entities.Device.DeviceOperation
        {
            DeviceCardId = createDto.DeviceCardId,
            OperationType = createDto.OperationType,
            StartTime = createDto.StartTime,
            EndTime = createDto.EndTime,
            Status = createDto.Status,
            Operator = createDto.Operator,
            ResultsJson = createDto.ResultsJson
        };
        await _repository.AddAsync(entity, cancellationToken);
        return await SaveChangesAsync(cancellationToken) ? MapToReadDto(entity) : null;
    }

    public async Task<bool> DeleteOperationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;
        _repository.Remove(entity);
        return await SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<DeviceOperationReadDto>> GetAllOperationsAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(MapToReadDto);
    }

    public async Task<IEnumerable<DeviceOperationReadDto>> GetOperationsByDeviceCardIdAsync(Guid deviceCardId, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.FindAsync(op => op.DeviceCardId == deviceCardId, cancellationToken);
        return entities.Select(MapToReadDto);
    }

    public async Task<DeviceOperationReadDto?> GetOperationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToReadDto(entity);
    }

    public async Task<bool> UpdateOperationAsync(Guid id, DeviceOperationUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;
        entity.EndTime = updateDto.EndTime;
        entity.Status = updateDto.Status;
        entity.ResultsJson = updateDto.ResultsJson;
        _repository.Update(entity);
        return await SaveChangesAsync(cancellationToken);
    }

    private static DeviceOperationReadDto MapToReadDto(Entities.Device.DeviceOperation e) =>
        new(e.Id, e.DeviceCardId, e.OperationType, e.StartTime, e.EndTime, e.Status, e.Operator, e.ResultsJson, e.CreatedAt, e.ModifiedAt);
}

// Example Skeleton for CategoryService
public class CategoryService : BaseDeviceService, ICategoryService
{
    private readonly ICategoryRepository _repository;
    public CategoryService(ICategoryRepository repository, ApplicationDbContext context, ILogger<CategoryService> logger)
        : base(logger, context)
    {
        _repository = repository;
    }

    public async Task<CategoryReadDto?> CreateCategoryAsync(CategoryCreateDto createDto, CancellationToken cancellationToken = default)
    {
         if (await _repository.ExistsAsync(c => c.Name == createDto.Name, cancellationToken))
         {
            _logger.LogWarning("Category creation failed: Name '{CategoryName}' already exists.", createDto.Name);
            return null;
         }
         var entity = new Entities.Device.Category { Name = createDto.Name, Description = createDto.Description };
         await _repository.AddAsync(entity, cancellationToken);
         return await SaveChangesAsync(cancellationToken) ? MapToReadDto(entity) : null;
    }
    public async Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;
        _repository.Remove(entity);
        return await SaveChangesAsync(cancellationToken);
    }
    public async Task<IEnumerable<CategoryReadDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(MapToReadDto);
    }
    public async Task<CategoryReadDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : MapToReadDto(entity);
    }
    public async Task<bool> UpdateCategoryAsync(Guid id, CategoryUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;
        // Check if name is being changed to one that already exists (excluding self)
        if (entity.Name != updateDto.Name && await _repository.ExistsAsync(c => c.Name == updateDto.Name && c.Id != id, cancellationToken))
        {
             _logger.LogWarning("Category update failed for ID {CategoryId}: Name '{CategoryName}' already exists.", id, updateDto.Name);
             return false;
        }

        entity.Name = updateDto.Name;
        entity.Description = updateDto.Description;
        _repository.Update(entity);
        return await SaveChangesAsync(cancellationToken);
    }
    private static CategoryReadDto MapToReadDto(Entities.Device.Category e) => new(e.Id, e.Name, e.Description);
} 