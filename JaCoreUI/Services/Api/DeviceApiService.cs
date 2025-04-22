using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JaCoreUI.Models.Device;
using Microsoft.Extensions.Configuration;
using JaCore.Common.Device;
using JaCore.Api.Dtos.Device;

namespace JaCoreUI.Services.Api;

/// <summary>
/// Service for interacting with the Device API endpoints
/// </summary>
public class DeviceApiService : ApiClientBase
{
    public DeviceApiService(IConfiguration configuration) : base(configuration)
    {
    }

    #region Devices
    
    /// <summary>
    /// Gets devices with pagination, ordered by last modified date
    /// </summary>
    public async Task<ObservableCollection<Models.Device.Device>> GetDevicesAsync(int page = 0, int pageSize = 10)
    {
        try
        {
            var deviceDtos = await GetAsync<List<JaCore.Api.Dtos.Device.DeviceDto>>($"Device?pageNumber={page}&pageSize={pageSize}");
            var devices = new ObservableCollection<Models.Device.Device>();
            if (deviceDtos != null) // Check if list is null
            {
                foreach (var dto in deviceDtos) { if (dto != null) { devices.Add(MapFromDto(dto)); } }
            }
            return devices;
        }
        catch (ApiException) { throw; }
        catch (Exception ex) { throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred while retrieving devices: {ex.Message}", ex); }
    }
    
    /// <summary>
    /// Gets a specific device by ID
    /// </summary>
    public async Task<Models.Device.Device?> GetDeviceByIdAsync(int id)
    {
        try
        {
            var dto = await GetAsync<JaCore.Api.Dtos.Device.DeviceDto?>($"Device/{id}");
            if (dto == null) return null;

            var device = MapFromDto(dto);
            
            // Load related operations separately (error handling within that call)
            ObservableCollection<DeviceOperation>? operations = null;
            try { operations = await GetDeviceOperationsForDeviceAsync(id); }
            catch (ApiException ex) { System.Diagnostics.Debug.WriteLine($"Non-critical error loading device operations for device {id}: {ex.Message}"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Unexpected error loading device operations for device {id}: {ex.Message}");}
             
            // Only update the IDs collection, as the DeviceOperations collection was removed from the model
            if(operations != null) device.DeviceOperationsIds = new ObservableCollection<int>(operations.Select(op => op.Id));

            return device;
        }
        catch (ApiException ex) when (ex.ErrorType == ApiErrorType.NotFound) { return null; }
        catch (ApiException) { throw; }
        catch (Exception ex) { throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred retrieving device {id}: {ex.Message}", ex); }
    }
    
    /// <summary>
    /// Creates a new device
    /// </summary>
    public async Task<Models.Device.Device> CreateDeviceAsync(Models.Device.Device device)
    {
        try
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device), "Device cannot be null");
            }
            
            var createDto = new JaCore.Api.Dtos.Device.CreateDeviceDto
            {
                Name = device.Name ?? string.Empty,
                Description = device.Description,
                DataState = device.DataState,
                OperationalState = device.OperationalState,
                CategoryId = device.Category?.Id,
                Properties = device.Properties != null && device.Properties.Count > 0 
                    ? JsonSerializer.Serialize(device.Properties) 
                    : null,
                OrderIndex = device.OrderIndex,
                IsCompleted = device.IsCompleted
            };
            
            var createdDto = await PostAsync<JaCore.Api.Dtos.Device.CreateDeviceDto, JaCore.Api.Dtos.Device.DeviceDto>("Device", createDto) ?? throw new ApiException(ApiErrorType.Unknown, "API did not return the created device.");
            return MapFromDto(createdDto);
        }
        catch (ApiException) { throw; }
        catch (Exception ex) { throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred creating device: {ex.Message}", ex); }
    }
    
    /// <summary>
    /// Updates an existing device
    /// </summary>
    public async Task UpdateDeviceAsync(Models.Device.Device device)
    {
        try
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device), "Device cannot be null");
            }
            
            var updateDto = new JaCore.Api.Dtos.Device.UpdateDeviceDto
            {
                Name = device.Name ?? string.Empty,
                Description = device.Description,
                DataState = device.DataState,
                OperationalState = device.OperationalState,
                CategoryId = device.Category?.Id,
                Properties = device.Properties != null && device.Properties.Count > 0 
                    ? JsonSerializer.Serialize(device.Properties) 
                    : null,
                OrderIndex = device.OrderIndex,
                IsCompleted = device.IsCompleted
            };
            
            await PutAsync($"Device/{device.Id}", updateDto);
        }
        catch (ApiException) { throw; }
        catch (Exception ex) { throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred updating device {device.Id}: {ex.Message}", ex); }
    }
    
    /// <summary>
    /// Deletes a device by ID
    /// </summary>
    public async Task DeleteDeviceAsync(int id)
    {
        try { await DeleteAsync($"Device/{id}"); }
        catch (ApiException) { throw; }
        catch (Exception ex) { throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred deleting device {id}: {ex.Message}", ex); }
    }
    
    #endregion
    
    #region Categories
    
    /// <summary>
    /// Gets categories with pagination
    /// </summary>
    public async Task<ObservableCollection<Category>> GetCategoriesAsync(int page = 0, int pageSize = 20)
    {
        try
        {
            var dtos = await GetAsync<List<JaCore.Api.Dtos.Device.CategoryDto>>($"Category?pageNumber={page}&pageSize={pageSize}");
            return new ObservableCollection<Category>(dtos?.Select(dto => new Category { Id = dto.Id, Name = dto.Name ?? string.Empty }) ?? new List<Category>());
        }
        catch (ApiException) { throw; }
        catch (Exception ex) { throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred retrieving categories: {ex.Message}", ex); }
    }
    
    /// <summary>
    /// Gets a specific category by ID
    /// </summary>
    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        try
        {
            var dto = await GetAsync<JaCore.Api.Dtos.Device.CategoryDto?>($"Category/{id}");
            return dto == null ? null : new Category { Id = dto.Id, Name = dto.Name ?? string.Empty };
        }
        catch (ApiException ex) when (ex.ErrorType == ApiErrorType.NotFound) { return null; }
        catch (ApiException) { throw; }
        catch (Exception ex) { throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred retrieving category {id}: {ex.Message}", ex); }
    }
    
    /// <summary>
    /// Creates a new category
    /// </summary>
    public async Task<Category> CreateCategoryAsync(Category category)
    {
        try
        {
            var createDto = new JaCore.Api.Dtos.Device.CreateCategoryDto { Name = category.Name ?? string.Empty };
            var dto = await PostAsync<JaCore.Api.Dtos.Device.CreateCategoryDto, JaCore.Api.Dtos.Device.CategoryDto>("Category", createDto) ?? throw new ApiException(ApiErrorType.Unknown, "API did not return the created category.");
            return new Category { Id = dto.Id, Name = dto.Name ?? string.Empty };
        }
        catch (ApiException) { throw; }
        catch (Exception ex) { throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred creating category: {ex.Message}", ex); }
    }
    
    /// <summary>
    /// Updates an existing category
    /// </summary>
    public async Task UpdateCategoryAsync(Category category)
    {
        try
        {
            await PutAsync($"Category/{category.Id}", new JaCore.Api.Dtos.Device.UpdateCategoryDto { Name = category.Name ?? string.Empty });
        }
        catch (ApiException) { throw; }
        catch (Exception ex) { throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred updating category {category.Id}: {ex.Message}", ex); }
    }
    
    /// <summary>
    /// Deletes a category by ID
    /// </summary>
    public async Task DeleteCategoryAsync(int id)
    {
        try { await DeleteAsync($"Category/{id}"); }
        catch (ApiException) { throw; }
        catch (Exception ex) { throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred deleting category {id}: {ex.Message}", ex); }
    }
    
    #endregion
    
    #region Suppliers
    
    /// <summary>
    /// Gets suppliers with pagination
    /// </summary>
    public async Task<ObservableCollection<Supplier>> GetSuppliersAsync(int page = 0, int pageSize = 20)
    {
        try
        {
            var dtos = await GetAsync<List<JaCore.Api.Dtos.Device.SupplierDto>>($"Supplier?pageNumber={page}&pageSize={pageSize}");
            return new ObservableCollection<Supplier>(dtos?.Select(dto => MapFromDto(dto)) ?? new List<Supplier>());
        }
        catch (ApiException) { throw; }
        catch (Exception ex) { throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred retrieving suppliers: {ex.Message}", ex); }
    }
    
    /// <summary>
    /// Gets a specific supplier by ID
    /// </summary>
    public async Task<Supplier?> GetSupplierByIdAsync(int id)
    {
        try
        {
            var dto = await GetAsync<JaCore.Api.Dtos.Device.SupplierDto?>($"Supplier/{id}");
            return dto == null ? null : MapFromDto(dto);
        }
        catch (ApiException ex) when (ex.ErrorType == ApiErrorType.NotFound) { return null; }
        catch (ApiException) { throw; }
        catch (Exception ex) { throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred retrieving supplier {id}: {ex.Message}", ex); }
    }
    
    /// <summary>
    /// Creates a new supplier
    /// </summary>
    public async Task<Supplier> CreateSupplierAsync(Supplier supplier)
    {
        try
        {
            var createDto = new JaCore.Api.Dtos.Device.CreateSupplierDto 
            { 
                Name = supplier.Name ?? string.Empty,
                Contact = supplier.Contact
            };
            
            var dto = await PostAsync<JaCore.Api.Dtos.Device.CreateSupplierDto, JaCore.Api.Dtos.Device.SupplierDto>("Supplier", createDto);
            
            return MapFromDto(dto);
        }
        catch (ApiException) { throw; }
        catch (Exception ex) { throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred creating supplier: {ex.Message}", ex); }
    }
    
    /// <summary>
    /// Updates an existing supplier
    /// </summary>
    public async Task UpdateSupplierAsync(Supplier supplier)
    {
        try
        {
            var updateDto = new JaCore.Api.Dtos.Device.UpdateSupplierDto 
            { 
                Name = supplier.Name ?? string.Empty,
                Contact = supplier.Contact
            };
            
            await PutAsync($"Supplier/{supplier.Id}", updateDto);
        }
        catch (ApiException) { throw; }
        catch (Exception ex) { throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred updating supplier {supplier.Id}: {ex.Message}", ex); }
    }
    
    /// <summary>
    /// Deletes a supplier by ID
    /// </summary>
    public async Task DeleteSupplierAsync(int id)
    {
        try { await DeleteAsync($"Supplier/{id}"); }
        catch (ApiException) { throw; }
        catch (Exception ex) { throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred deleting supplier {id}: {ex.Message}", ex); }
    }
    
    #endregion
    
    #region Services
    
    /// <summary>
    /// Gets services with pagination
    /// </summary>
    public async Task<ObservableCollection<Service>> GetServicesAsync(int page = 0, int pageSize = 20)
    {
        try
        {
            var dtos = await GetAsync<List<JaCore.Api.Dtos.Device.ServiceDto>>($"Service?pageNumber={page}&pageSize={pageSize}");
            
            return new ObservableCollection<Service>(dtos.Select(dto => new Service
            {
                Id = dto.Id,
                Name = dto.Name ?? string.Empty,
                Contact = dto.Contact
            }));
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Error retrieving services: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Gets a specific service by ID
    /// </summary>
    public async Task<Service?> GetServiceByIdAsync(int id)
    {
        try
        {
            var dto = await GetAsync<JaCore.Api.Dtos.Device.ServiceDto?>($"Service/{id}");
            return dto == null ? null : new Service
            {
                Id = dto.Id,
                Name = dto.Name ?? string.Empty,
                Contact = dto.Contact
            };
        }
        catch (ApiException ex) when (ex.ErrorType == ApiErrorType.NotFound) { return null; }
        catch (ApiException) { throw; }
        catch (Exception ex) { throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred retrieving service {id}: {ex.Message}", ex); }
    }
    
    /// <summary>
    /// Creates a new service
    /// </summary>
    public async Task<Service> CreateServiceAsync(Service service)
    {
        try
        {
            var createDto = new JaCore.Api.Dtos.Device.CreateServiceDto 
            { 
                Name = service.Name ?? string.Empty,
                Contact = service.Contact
            };
            
            var dto = await PostAsync<JaCore.Api.Dtos.Device.CreateServiceDto, JaCore.Api.Dtos.Device.ServiceDto>("Service", createDto);
            
            return new Service
            {
                Id = dto.Id,
                Name = dto.Name ?? string.Empty,
                Contact = dto.Contact
            };
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Error creating service: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Updates an existing service
    /// </summary>
    public async Task UpdateServiceAsync(Service service)
    {
        try
        {
            var updateDto = new JaCore.Api.Dtos.Device.UpdateServiceDto 
            { 
                Name = service.Name ?? string.Empty,
                Contact = service.Contact
            };
            
            await PutAsync($"Service/{service.Id}", updateDto);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Error updating service: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Deletes a service by ID
    /// </summary>
    public async Task DeleteServiceAsync(int id)
    {
        try
        {
            await DeleteAsync($"Service/{id}");
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Error deleting service: {ex.Message}", ex);
        }
    }
    
    #endregion
    
    #region Device Operations
    
    /// <summary>
    /// Gets device operations for a specific device
    /// </summary>
    public async Task<ObservableCollection<DeviceOperation>> GetDeviceOperationsForDeviceAsync(int deviceId)
    {
        Models.Device.Device? parentDevice = null;
        try { parentDevice = await GetDeviceByIdAsync(deviceId); }
        catch { /* Ignore if device lookup fails */ }

        if (parentDevice == null || parentDevice.DeviceCardId <= 0)
        {
            return new ObservableCollection<DeviceOperation>(); // No card, no operations
        }
        var deviceCardId = parentDevice.DeviceCardId;

        try
        {
            var dtos = await GetAsync<List<JaCore.Api.Dtos.Device.DeviceOperationDto>>($"DeviceOperation?deviceCardId={deviceCardId}");
            
            var operations = new ObservableCollection<DeviceOperation>();
            if (dtos != null)
            { foreach (var dto in dtos) { if (dto != null) { operations.Add(MapFromDto(dto, deviceId)); } } }
            return operations;
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred retrieving device operations for device {deviceId}: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Gets a specific device operation by ID
    /// </summary>
    public async Task<DeviceOperation?> GetDeviceOperationByIdAsync(int id)
    {
        try
        {
            var dto = await GetAsync<JaCore.Api.Dtos.Device.DeviceOperationDto?>($"DeviceOperation/{id}");
            if (dto == null) return null;
            return MapFromDto(dto, 0);
        }
        catch (ApiException ex) when (ex.ErrorType == ApiErrorType.NotFound) { return null; }
        catch (ApiException) { throw; }
        catch (Exception ex) { throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred retrieving device operation {id}: {ex.Message}", ex); }
    }
    
    /// <summary>
    /// Creates a new device operation
    /// </summary>
    public async Task<DeviceOperation> CreateDeviceOperationAsync(DeviceOperation operation, int deviceCardId)
    {
        if (deviceCardId <= 0)
        {
             throw new ArgumentException("A valid DeviceCardId must be provided to create a DeviceOperation.", nameof(deviceCardId));
        }

        try
        {
            var createDto = new JaCore.Api.Dtos.Device.CreateDeviceOperationDto 
            { 
                Name = operation.Name ?? string.Empty,
                Description = operation.Description,
                OrderIndex = operation.OrderIndex,
                DeviceCardId = deviceCardId,
                IsRequired = operation.IsRequired,
                IsCompleted = operation.IsCompleted,
                UiElements = null
            };
            
            var returnedDto = await PostAsync<JaCore.Api.Dtos.Device.CreateDeviceOperationDto, JaCore.Api.Dtos.Device.DeviceOperationDto>("DeviceOperation", createDto);
            return MapFromDto(returnedDto, operation.DeviceId);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Error creating device operation: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Updates an existing device operation
    /// </summary>
    public async Task UpdateDeviceOperationAsync(DeviceOperation operation)
    {
        try
        {
            var updateDto = new JaCore.Api.Dtos.Device.UpdateDeviceOperationDto 
            { 
                Name = operation.Name ?? string.Empty,
                Description = operation.Description,
                OrderIndex = operation.OrderIndex,
                IsRequired = operation.IsRequired,
                IsCompleted = operation.IsCompleted,
                UiElements = null
            };
            
            await PutAsync($"DeviceOperation/{operation.Id}", updateDto);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Error updating device operation: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Deletes a device operation by ID
    /// </summary>
    public async Task DeleteDeviceOperationAsync(int id)
    {
        try
        {
            await DeleteAsync($"DeviceOperation/{id}");
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Error deleting device operation: {ex.Message}", ex);
        }
    }
    
    #endregion
    
    #region Device Cards
    
    /// <summary>
    /// Gets device cards with pagination
    /// </summary>
    public async Task<ObservableCollection<DeviceCard>> GetDeviceCardsAsync(int page = 0, int pageSize = 20)
    {
        try
        {
            var dtos = await GetAsync<List<JaCore.Api.Dtos.Device.DeviceCardDto>>($"DeviceCard?pageNumber={page}&pageSize={pageSize}");
            
            var deviceCards = new ObservableCollection<DeviceCard>();
            
            foreach (var dto in dtos)
            {
                var deviceCard = MapFromDto(dto);
                
                try 
                {
                    if (dto.SupplierId.HasValue && dto.SupplierId > 0)
                    {
                        deviceCard.Supplier = await GetSupplierByIdAsync(dto.SupplierId.Value);
                    }
                    
                    if (dto.ServiceId.HasValue && dto.ServiceId > 0)
                    {
                        deviceCard.Service = await GetServiceByIdAsync(dto.ServiceId.Value);
                    }
                }
                catch { /* Ignore errors loading references */ }
                
                deviceCards.Add(deviceCard);
            }
            
            return deviceCards;
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Error retrieving device cards: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Gets a specific device card by ID
    /// </summary>
    public async Task<DeviceCard?> GetDeviceCardByIdAsync(int id)
    {
        try
        {
            var dto = await GetAsync<JaCore.Api.Dtos.Device.DeviceCardDto?>($"DeviceCard/{id}");
            if (dto == null) return null;

            var deviceCard = MapFromDto(dto);

            try 
            {
                if (dto.SupplierId.HasValue && dto.SupplierId > 0)
                {
                    deviceCard.Supplier = await GetSupplierByIdAsync(dto.SupplierId.Value);
                }
                
                if (dto.ServiceId.HasValue && dto.ServiceId > 0)
                {
                    deviceCard.Service = await GetServiceByIdAsync(dto.ServiceId.Value);
                }
            }
            catch { /* Ignore errors loading references */ }
            
            return deviceCard;
        }
        catch (ApiException ex) when (ex.ErrorType == ApiErrorType.NotFound) { return null; }
        catch (ApiException) { throw; }
        catch (Exception ex) { throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred retrieving device card {id}: {ex.Message}", ex); }
    }
    
    /// <summary>
    /// Creates a new device card
    /// </summary>
    public async Task<DeviceCard> CreateDeviceCardAsync(DeviceCard deviceCard)
    {
        try
        {
            var createDto = new JaCore.Api.Dtos.Device.CreateDeviceCardDto
            {
                SerialNumber = deviceCard.SerialNumber ?? string.Empty,
                DateOfActivation = deviceCard.DateOfActivation ?? DateTimeOffset.Now,
                SupplierId = deviceCard.SupplierId,
                ServiceId = deviceCard.ServiceId,
                MetConLevel1 = deviceCard.MetrologicalConformation?.Level1,
                MetConLevel2 = deviceCard.MetrologicalConformation?.Level2,
                MetConLevel3 = deviceCard.MetrologicalConformation?.Level3,
                MetConLevel4 = deviceCard.MetrologicalConformation?.Level4
            };
            
            var returnedDto = await PostAsync<JaCore.Api.Dtos.Device.CreateDeviceCardDto, JaCore.Api.Dtos.Device.DeviceCardDto>("DeviceCard", createDto);
            return MapFromDto(returnedDto);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Error creating device card: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Updates an existing device card
    /// </summary>
    public async Task UpdateDeviceCardAsync(DeviceCard deviceCard)
    {
        try
        {
            if (deviceCard.Id == 0) throw new ApiException(ApiErrorType.BadRequest, "Device card ID is missing for update");

            var updateDto = new JaCore.Api.Dtos.Device.UpdateDeviceCardDto
            {
                SerialNumber = deviceCard.SerialNumber ?? string.Empty,
                DateOfActivation = deviceCard.DateOfActivation ?? DateTimeOffset.Now,
                SupplierId = deviceCard.SupplierId,
                ServiceId = deviceCard.ServiceId,
                MetConLevel1 = deviceCard.MetrologicalConformation?.Level1,
                MetConLevel2 = deviceCard.MetrologicalConformation?.Level2,
                MetConLevel3 = deviceCard.MetrologicalConformation?.Level3,
                MetConLevel4 = deviceCard.MetrologicalConformation?.Level4
            };
            
            await PutAsync($"DeviceCard/{deviceCard.Id}", updateDto);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Error updating device card: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Deletes a device card by ID
    /// </summary>
    public async Task DeleteDeviceCardAsync(int id)
    {
        try
        {
            await DeleteAsync($"DeviceCard/{id}");
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Error deleting device card: {ex.Message}", ex);
        }
    }
    
    #endregion
    
    #region Events

    /// <summary>
    /// Gets events for a specific device card
    /// </summary>
    public async Task<ObservableCollection<Event>> GetEventsForDeviceCardAsync(int deviceCardId)
    {
        if (deviceCardId <= 0)
        {
            return new ObservableCollection<Event>(); // No card, no events
        }

        try
        {
            var dtos = await GetAsync<List<JaCore.Api.Dtos.Device.EventDto>>($"Event?deviceCardId={deviceCardId}");
            
            var events = new ObservableCollection<Event>();
            if (dtos != null)
            { 
                foreach (var dto in dtos) 
                { 
                    if (dto != null) { events.Add(MapFromDto(dto)); } 
                }
            }
            return events;
        }
        catch (ApiException) { throw; }
        catch (Exception ex) { throw new ApiException(ApiErrorType.Unknown, $"An unexpected error occurred retrieving events for device card {deviceCardId}: {ex.Message}", ex); }
    }

    // TODO: Add CreateEventAsync, UpdateEventAsync, DeleteEventAsync if needed by UI

    #endregion
    
    #region Helpers
    
    private Models.Device.Device MapFromDto(JaCore.Api.Dtos.Device.DeviceDto dto)
    {
        Dictionary<string, string>? properties = null;
        if (!string.IsNullOrEmpty(dto.Properties))
        {
            try { properties = JsonSerializer.Deserialize<Dictionary<string, string>>(dto.Properties); }
            catch (JsonException ex) { System.Diagnostics.Debug.WriteLine($"Error deserializing properties for device {dto.Id}: {ex.Message}"); }
        }

        return new Models.Device.Device
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            DataState = dto.DataState,
            OperationalState = dto.OperationalState,
            CreatedAt = dto.CreatedAt,
            ModifiedAt = dto.ModifiedAt,
            Category = dto.CategoryId.HasValue ? new Category { Id = dto.CategoryId.Value } : null,
            DeviceCardId = dto.DeviceCardId ?? 0,
            Properties = properties ?? new Dictionary<string, string>(),
            OrderIndex = dto.OrderIndex,
            IsCompleted = dto.IsCompleted
        };
    }

    private Models.Device.DeviceCard MapFromDto(JaCore.Api.Dtos.Device.DeviceCardDto dto)
    {
        return new Models.Device.DeviceCard
        {
            Id = dto.Id,
            SerialNumber = dto.SerialNumber,
            DateOfActivation = dto.DateOfActivation,
            SupplierId = dto.SupplierId,
            ServiceId = dto.ServiceId,
            MetrologicalConformation = new JaCore.Common.Device.MetrologicalConformation
            {
                Level1 = dto.MetConLevel1 ?? string.Empty,
                Level2 = dto.MetConLevel2 ?? string.Empty,
                Level3 = dto.MetConLevel3 ?? string.Empty,
                Level4 = dto.MetConLevel4 ?? string.Empty
            }
        };
    }

    private Models.Device.DeviceOperation MapFromDto(JaCore.Api.Dtos.Device.DeviceOperationDto dto, int deviceId)
    {
        if (deviceId <= 0) 
        {
             System.Diagnostics.Debug.WriteLine($"Warning: Mapping DeviceOperation {dto.Id} without a valid original DeviceId.");
        }

        return new Models.Device.DeviceOperation
        {
            Id = dto.Id,
            Name = dto.Name ?? string.Empty,
            Description = dto.Description,
            OrderIndex = dto.OrderIndex,
            DeviceId = deviceId,
            IsRequired = dto.IsRequired,
            IsCompleted = dto.IsCompleted,
            UiElements = new ObservableCollection<Models.UI.Base.UIElement>()
        };
    }

    private Event MapFromDto(JaCore.Api.Dtos.Device.EventDto dto) => new Event
    {
        Id = dto.Id,
        Type = dto.Type,
        Who = dto.Who,
        From = dto.From,
        To = dto.To,
        Description = dto.Description
        // Note: DeviceCardId from DTO is not directly mapped to the UI Event model
    };

    private Supplier MapFromDto(JaCore.Api.Dtos.Device.SupplierDto dto) => new Supplier { Id = dto.Id, Name = dto.Name, Contact = dto.Contact };
    
    #endregion
}

