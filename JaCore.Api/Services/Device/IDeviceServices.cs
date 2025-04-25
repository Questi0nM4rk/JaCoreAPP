using JaCore.Api.DTOs.Device;

namespace JaCore.Api.Services.Device;

// Define interfaces for each entity's service
// These would typically return Result<T> or similar for error handling

public interface IDeviceService
{
    Task<DeviceReadDto?> GetDeviceByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DeviceReadDto>> GetAllDevicesAsync(CancellationToken cancellationToken = default);
    Task<DeviceReadDto?> CreateDeviceAsync(DeviceCreateDto createDto, CancellationToken cancellationToken = default);
    Task<bool> UpdateDeviceAsync(Guid id, DeviceUpdateDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteDeviceAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DeviceReadDto?> GetDeviceBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default);
}

public interface IDeviceCardService
{
    Task<DeviceCardReadDto?> GetDeviceCardByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DeviceCardReadDto>> GetAllDeviceCardsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<DeviceCardReadDto>> GetDeviceCardsByDeviceIdAsync(Guid deviceId, CancellationToken cancellationToken = default);
    Task<DeviceCardReadDto?> CreateDeviceCardAsync(DeviceCardCreateDto createDto, CancellationToken cancellationToken = default);
    Task<bool> UpdateDeviceCardAsync(Guid id, DeviceCardUpdateDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteDeviceCardAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ICategoryService
{
    Task<CategoryReadDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CategoryReadDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<CategoryReadDto?> CreateCategoryAsync(CategoryCreateDto createDto, CancellationToken cancellationToken = default);
    Task<bool> UpdateCategoryAsync(Guid id, CategoryUpdateDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ISupplierService
{
    Task<SupplierReadDto?> GetSupplierByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SupplierReadDto>> GetAllSuppliersAsync(CancellationToken cancellationToken = default);
    Task<SupplierReadDto?> CreateSupplierAsync(SupplierCreateDto createDto, CancellationToken cancellationToken = default);
    Task<bool> UpdateSupplierAsync(Guid id, SupplierUpdateDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteSupplierAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IServiceService // Renamed from IServiceEntityService
{
    Task<ServiceReadDto?> GetServiceByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceReadDto>> GetAllServicesAsync(CancellationToken cancellationToken = default);
    Task<ServiceReadDto?> CreateServiceAsync(ServiceCreateDto createDto, CancellationToken cancellationToken = default);
    Task<bool> UpdateServiceAsync(Guid id, ServiceUpdateDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteServiceAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IEventService
{
    Task<EventReadDto?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventReadDto>> GetAllEventsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<EventReadDto>> GetEventsByDeviceCardIdAsync(Guid deviceCardId, CancellationToken cancellationToken = default);
    Task<EventReadDto?> CreateEventAsync(EventCreateDto createDto, CancellationToken cancellationToken = default);
    // Delete might be relevant depending on requirements
    Task<bool> DeleteEventAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IDeviceOperationService
{
    Task<DeviceOperationReadDto?> GetOperationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DeviceOperationReadDto>> GetAllOperationsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<DeviceOperationReadDto>> GetOperationsByDeviceCardIdAsync(Guid deviceCardId, CancellationToken cancellationToken = default);
    Task<DeviceOperationReadDto?> CreateOperationAsync(DeviceOperationCreateDto createDto, CancellationToken cancellationToken = default);
    Task<bool> UpdateOperationAsync(Guid id, DeviceOperationUpdateDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteOperationAsync(Guid id, CancellationToken cancellationToken = default);
} 