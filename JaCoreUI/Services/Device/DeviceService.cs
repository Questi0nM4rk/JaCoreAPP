using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Data;
using JaCoreUI.Models.Device;
using JaCoreUI.Models.UI;
using JaCoreUI.Services.Api;
using MsBox.Avalonia.Enums;
using JaCore.Common.Device;

namespace JaCoreUI.Services.Device;

/// <summary>
/// Service for device-related operations
/// </summary>
public partial class DeviceService : ObservableObject
{
    private readonly DeviceApiService _deviceApiService;
    
    [ObservableProperty]
    private ObservableCollection<Models.Device.Device> _devices = new();
    
    [ObservableProperty]
    private Models.Device.Device? _tempDevice;
    
    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();
    
    [ObservableProperty]
    private ObservableCollection<Supplier> _suppliers = new();
    
    [ObservableProperty]
    private ObservableCollection<Models.Device.Service> _services = new();
    
    [ObservableProperty]
    private ObservableCollection<DeviceCard> _deviceCards = new();
    
    public DeviceService(DeviceApiService deviceApiService)
    {
        _deviceApiService = deviceApiService;
        LoadInitialDataAsync();
    }
    
    // Wrapper function to show errors
    private async Task ShowApiErrorDialog(string context, Exception ex)
    {
        string errorMessage = ex is ApiException apiEx 
            ? $"API Error ({apiEx.ErrorType}) while {context}:\n{apiEx.Message}" 
            : $"Unexpected error while {context}:\n{ex.Message}";
        
        // Log full details for debugging
        System.Diagnostics.Debug.WriteLine($"Error Context: {context}\n{ex}"); 
        
        await ErrorDialog.ShowWithButtonsAsync(errorMessage, "API Error", ButtonEnum.Ok, Icon.Error);
    }

    private async void LoadInitialDataAsync()
    {
        // Keep loading devices for now, handle error
        try
        {
            // Use the service method which now includes error handling
            Devices = await GetDevicesAsync(); 
        }
        catch (Exception ex) // Catch should technically not be needed if GetDevicesAsync handles it
        {                    // But keep as safety net for unexpected sync/void issues
            await ShowApiErrorDialog("loading initial devices", ex);
            Devices = new ObservableCollection<Models.Device.Device>(); // Ensure collection is initialized
        }

        // Defer loading of other lists
        // Categories = await GetCategoriesAsync();
        // Suppliers = await GetSuppliersAsync();
        // Services = await GetServicesAsync();
        // DeviceCards = await GetDeviceCardsAsync();
    }
    
    [RelayCommand]
    public async Task CreateNewDevice()
    {
        TempDevice = CreateDeviceTemplate();
        // UI navigation would typically happen here or be triggered by an event
        await Task.Delay(1); // Add minimal await to make it truly async
    }
    
    [RelayCommand]
    public async Task OpenDevice(int id)
    {   
        // Reset TempDevice before loading
        TempDevice = null; 
        Models.Device.Device? loadedDevice = null;
        try
        {
            loadedDevice = await _deviceApiService.GetDeviceByIdAsync(id);
            TempDevice = loadedDevice; // Assign only if successful
        }
        catch (Exception ex)
        {   
            await ShowApiErrorDialog($"opening device {id}", ex);
        } 
        // UI navigation might depend on TempDevice not being null
    }
    
    [RelayCommand]
    public async Task SaveDevice()
    {
        if (TempDevice == null) return;
        bool success = false;
        Models.Device.Device? resultDevice = null;
        string operationContext = TempDevice.Id == 0 ? "creating device" : $"updating device {TempDevice.Id}";

        try
        {
            if (TempDevice.Id == 0)
            {
                resultDevice = await _deviceApiService.CreateDeviceAsync(TempDevice);
                success = resultDevice != null;
                if (success && resultDevice != null)
                {
                    await Dispatcher.UIThread.InvokeAsync(() => { Devices.Add(resultDevice); });
                    TempDevice = resultDevice; // Update TempDevice with the one returned from API (includes ID)
                }
            }
            else
            {
                await _deviceApiService.UpdateDeviceAsync(TempDevice);
                success = true; // Assume success if no exception
                if (success)
                {
                     await Dispatcher.UIThread.InvokeAsync(() =>
                     {
                         var index = Devices.IndexOf(Devices.FirstOrDefault(d => d.Id == TempDevice.Id)!);
                         if (index >= 0) { Devices[index] = TempDevice; }
                         else { Devices.Add(TempDevice); } // Add if somehow missing
                     });
                }
            }
        }
        catch (Exception ex)
        {
            await ShowApiErrorDialog(operationContext, ex);
            success = false;
        }

        if(success)
        {
             // Optionally show success message or navigate
        }
    }
    
    [RelayCommand]
    public async Task DeleteDevice(int id)
    {
        // Optional: Confirmation dialog before deleting
        var confirmResult = await ErrorDialog.ShowWithButtonsAsync($"Are you sure you want to delete device {id}?", 
                                                               "Confirm Deletion", ButtonEnum.YesNo, Icon.Question);
        if (confirmResult != ButtonResult.Yes) return;

        bool success = false;
        try
        {
            await _deviceApiService.DeleteDeviceAsync(id);
            success = true;
             await Dispatcher.UIThread.InvokeAsync(() =>
             {
                 var device = Devices.FirstOrDefault(d => d.Id == id);
                 if (device != null) { Devices.Remove(device); }
                 if (TempDevice?.Id == id) { TempDevice = null; } // Clear temp if it was the deleted one
             });
        }
        catch (Exception ex)
        {
            await ShowApiErrorDialog($"deleting device {id}", ex);
        }
    }
    
    /// <summary>
    /// Gets all devices with pagination
    /// </summary>
    public async Task<ObservableCollection<Models.Device.Device>> GetDevicesAsync(int page = 0, int pageSize = 10)
    {
        try { return await _deviceApiService.GetDevicesAsync(page, pageSize); }
        catch (Exception ex) 
        { 
            await ShowApiErrorDialog("getting devices", ex);
            return new ObservableCollection<Models.Device.Device>(); // Return empty on error
        }
    }
    
    /// <summary>
    /// Gets a device by ID
    /// </summary>
    public async Task<Models.Device.Device?> GetDeviceByIdAsync(int id)
    {
        try { return await _deviceApiService.GetDeviceByIdAsync(id); }
        catch (Exception ex) 
        { 
            await ShowApiErrorDialog($"getting device {id}", ex);
            return null; // Return null on error
        }
    }
    
    /// <summary>
    /// Creates a new device
    /// </summary>
    public async Task<Models.Device.Device?> CreateDeviceAsync(Models.Device.Device device)
    {
        try { return await _deviceApiService.CreateDeviceAsync(device); }
        catch (Exception ex) 
        { 
            await ShowApiErrorDialog("creating device", ex);
            return null; // Return null on error
        }
    }
    
    /// <summary>
    /// Updates an existing device
    /// </summary>
    public async Task<bool> UpdateDeviceAsync(Models.Device.Device device)
    {
        try 
        { 
            await _deviceApiService.UpdateDeviceAsync(device); 
            return true; 
        }
        catch (Exception ex) 
        { 
            await ShowApiErrorDialog($"updating device {device.Id}", ex);
            return false; // Return false on error
        }
    }
    
    /// <summary>
    /// Deletes a device by ID
    /// </summary>
    public async Task<bool> DeleteDeviceAsync(int id)
    {
        try 
        { 
            await _deviceApiService.DeleteDeviceAsync(id); 
            return true; 
        }
        catch (Exception ex) 
        { 
            await ShowApiErrorDialog($"deleting device {id}", ex);
            return false; // Return false on error
        }
    }
    
    /// <summary>
    /// Gets all categories
    /// </summary>
    public async Task<ObservableCollection<Category>> GetCategoriesAsync(bool forceRefresh = false)
    {
        if (!forceRefresh && Categories.Count > 0) // Check if already loaded
        {
            return Categories;
        }

        try 
        { 
            var categories = await _deviceApiService.GetCategoriesAsync(); 
            Categories = categories; // Update the observable collection
            return Categories;
        }
        catch (Exception ex) { await ShowApiErrorDialog("getting categories", ex); Categories ??= new ObservableCollection<Category>(); return Categories; }
    }
    
    /// <summary>
    /// Gets a category by ID
    /// </summary>
    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        try { return await _deviceApiService.GetCategoryByIdAsync(id); }
        catch (Exception ex) { await ShowApiErrorDialog($"getting category {id}", ex); return null; }
    }
    
    /// <summary>
    /// Creates a new category
    /// </summary>
    public async Task<Category?> CreateCategoryAsync(Category category)
    {
        try { return await _deviceApiService.CreateCategoryAsync(category); }
        catch (Exception ex) { await ShowApiErrorDialog("creating category", ex); return null; }
    }
    
    /// <summary>
    /// Updates an existing category
    /// </summary>
    public async Task<bool> UpdateCategoryAsync(Category category)
    {
        try { await _deviceApiService.UpdateCategoryAsync(category); return true; }
        catch (Exception ex) { await ShowApiErrorDialog($"updating category {category.Id}", ex); return false; }
    }
    
    /// <summary>
    /// Deletes a category by ID
    /// </summary>
    public async Task<bool> DeleteCategoryAsync(int id)
    {
        // Optional: Confirmation dialog
        var confirmResult = await ErrorDialog.ShowWithButtonsAsync($"Are you sure you want to delete category {id}?", "Confirm Deletion", ButtonEnum.YesNo, Icon.Question);
        if (confirmResult != ButtonResult.Yes) return false;

        try { await _deviceApiService.DeleteCategoryAsync(id); return true; }
        catch (Exception ex) { await ShowApiErrorDialog($"deleting category {id}", ex); return false; }
    }
    
    /// <summary>
    /// Gets all suppliers
    /// </summary>
    public async Task<ObservableCollection<Supplier>> GetSuppliersAsync(bool forceRefresh = false)
    {
        if (!forceRefresh && Suppliers.Count > 0) 
        {
            return Suppliers;
        }
        
        try 
        { 
            var suppliers = await _deviceApiService.GetSuppliersAsync();
            Suppliers = suppliers; 
            return Suppliers;
        }
        catch (Exception ex) { await ShowApiErrorDialog("getting suppliers", ex); Suppliers ??= new ObservableCollection<Supplier>(); return Suppliers; }
    }
    
    /// <summary>
    /// Gets a supplier by ID
    /// </summary>
    public async Task<Supplier?> GetSupplierByIdAsync(int id)
    {
        try { return await _deviceApiService.GetSupplierByIdAsync(id); }
        catch (Exception ex) { await ShowApiErrorDialog($"getting supplier {id}", ex); return null; }
    }
    
    /// <summary>
    /// Creates a new supplier
    /// </summary>
    public async Task<Supplier?> CreateSupplierAsync(Supplier supplier)
    {
         try { return await _deviceApiService.CreateSupplierAsync(supplier); }
         catch (Exception ex) { await ShowApiErrorDialog("creating supplier", ex); return null; }
    }
    
    /// <summary>
    /// Updates an existing supplier
    /// </summary>
    public async Task<bool> UpdateSupplierAsync(Supplier supplier)
    {
        try { await _deviceApiService.UpdateSupplierAsync(supplier); return true; }
        catch (Exception ex) { await ShowApiErrorDialog($"updating supplier {supplier.Id}", ex); return false; }
    }
    
    /// <summary>
    /// Deletes a supplier by ID
    /// </summary>
    public async Task<bool> DeleteSupplierAsync(int id)
    {
        var confirmResult = await ErrorDialog.ShowWithButtonsAsync($"Are you sure you want to delete supplier {id}?", "Confirm Deletion", ButtonEnum.YesNo, Icon.Question);
        if (confirmResult != ButtonResult.Yes) return false;

        try { await _deviceApiService.DeleteSupplierAsync(id); return true; }
        catch (Exception ex) { await ShowApiErrorDialog($"deleting supplier {id}", ex); return false; }
    }
    
    /// <summary>
    /// Gets all services
    /// </summary>
    public async Task<ObservableCollection<Service>> GetServicesAsync(bool forceRefresh = false)
    {
         if (!forceRefresh && Services.Count > 0) 
        {
            return Services;
        }

        try 
        { 
            var services = await _deviceApiService.GetServicesAsync();
            Services = services;
            return Services;
        }
        catch (Exception ex) { await ShowApiErrorDialog("getting services", ex); Services ??= new ObservableCollection<Service>(); return Services; }
    }
    
    /// <summary>
    /// Gets a service by ID
    /// </summary>
    public async Task<Service?> GetServiceByIdAsync(int id)
    {
        try { return await _deviceApiService.GetServiceByIdAsync(id); }
        catch (Exception ex) { await ShowApiErrorDialog($"getting service {id}", ex); return null; }
    }
    
    /// <summary>
    /// Creates a new service
    /// </summary>
    public async Task<Service?> CreateServiceAsync(Service service)
    {
        try { return await _deviceApiService.CreateServiceAsync(service); }
        catch (Exception ex) { await ShowApiErrorDialog("creating service", ex); return null; }
    }
    
    /// <summary>
    /// Updates an existing service
    /// </summary>
    public async Task<bool> UpdateServiceAsync(Service service)
    {
         try { await _deviceApiService.UpdateServiceAsync(service); return true; }
         catch (Exception ex) { await ShowApiErrorDialog($"updating service {service.Id}", ex); return false; }
    }
    
    /// <summary>
    /// Deletes a service by ID
    /// </summary>
    public async Task<bool> DeleteServiceAsync(int id)
    {   
        var confirmResult = await ErrorDialog.ShowWithButtonsAsync($"Are you sure you want to delete service {id}?", "Confirm Deletion", ButtonEnum.YesNo, Icon.Question);
        if (confirmResult != ButtonResult.Yes) return false;

        try { await _deviceApiService.DeleteServiceAsync(id); return true; }
        catch (Exception ex) { await ShowApiErrorDialog($"deleting service {id}", ex); return false; }
    }
    
    /// <summary>
    /// Gets all device cards
    /// </summary>
    public async Task<ObservableCollection<DeviceCard>> GetDeviceCardsAsync(bool forceRefresh = false)
    {
        if (!forceRefresh && DeviceCards.Count > 0) 
        {
            return DeviceCards;
        }
        
        try 
        { 
             var cards = await _deviceApiService.GetDeviceCardsAsync();
             DeviceCards = cards;
             return DeviceCards;
        }
        catch (Exception ex) { await ShowApiErrorDialog("getting device cards", ex); DeviceCards ??= new ObservableCollection<DeviceCard>(); return DeviceCards; }
    }
    
    /// <summary>
    /// Gets a device card by ID
    /// </summary>
    public async Task<DeviceCard?> GetDeviceCardByIdAsync(int id)
    {
        try { return await _deviceApiService.GetDeviceCardByIdAsync(id); }
        catch (Exception ex) { await ShowApiErrorDialog($"getting device card {id}", ex); return null; }
    }
    
    /// <summary>
    /// Creates a new device card for a device
    /// </summary>
    public async Task<DeviceCard?> CreateDeviceCardForDeviceAsync(DeviceCard deviceCard, int deviceId)
    {
        // API Service needs modification to accept deviceId and use correct endpoint
        // For now, assume _deviceApiService.CreateDeviceCardAsync handles it (needs update)
        try { return await _deviceApiService.CreateDeviceCardAsync(deviceCard /*, deviceId */); } 
        catch (Exception ex) { await ShowApiErrorDialog($"creating device card for device {deviceId}", ex); return null; }
    }
    
    /// <summary>
    /// Updates an existing device card
    /// </summary>
    public async Task<bool> UpdateDeviceCardAsync(DeviceCard deviceCard)
    {
        try { await _deviceApiService.UpdateDeviceCardAsync(deviceCard); return true; }
        catch (Exception ex) { await ShowApiErrorDialog($"updating device card {deviceCard.Id}", ex); return false; }
    }
    
    /// <summary>
    /// Deletes a device card by ID
    /// </summary>
    public async Task<bool> DeleteDeviceCardAsync(int id)
    {
        var confirmResult = await ErrorDialog.ShowWithButtonsAsync($"Are you sure you want to delete device card {id}?", "Confirm Deletion", ButtonEnum.YesNo, Icon.Question);
        if (confirmResult != ButtonResult.Yes) return false;

        try { await _deviceApiService.DeleteDeviceCardAsync(id); return true; }
        catch (Exception ex) { await ShowApiErrorDialog($"deleting device card {id}", ex); return false; }
    }
    
    /// <summary>
    /// Creates a new device template with default values
    /// </summary>
    /// <returns>A new device with default values</returns>
    public Models.Device.Device CreateDeviceTemplate()
    {
        var device = new Models.Device.Device
        {
            Name = "New Device",
            Description = "Device Description",
            DataState = DeviceDataState.New,
            OperationalState = DeviceOperationalState.Active,
            CreatedAt = DateTime.Now,
            ModifiedAt = DateTime.Now,
            DeviceOperationsIds = new ObservableCollection<int>(),
            Properties = new Dictionary<string, string>()
        };
        
        return device;
    }

    // Note: GetDeviceOperationsForDeviceAsync is specific to a device,
    // so it doesn't fit the pattern of caching a global list here.
    // ViewModels should call this directly when needed for a specific device.
    // Ensure it has error handling:
    public async Task<ObservableCollection<DeviceOperation>> GetDeviceOperationsForDeviceAsync(int deviceId)
    {
         try { return await _deviceApiService.GetDeviceOperationsForDeviceAsync(deviceId); }
         catch (Exception ex) { await ShowApiErrorDialog($"getting operations for device {deviceId}", ex); return new ObservableCollection<DeviceOperation>(); }
    }

    // Method to get Events for a specific DeviceCard
    public async Task<ObservableCollection<Event>> GetEventsForDeviceCardAsync(int deviceCardId)
    {
        try { return await _deviceApiService.GetEventsForDeviceCardAsync(deviceCardId); }
        catch (Exception ex) { await ShowApiErrorDialog($"getting events for device card {deviceCardId}", ex); return new ObservableCollection<Event>(); }
    }

    /// <summary>
    /// Gets a device operation by ID
    /// </summary>
    public async Task<DeviceOperation?> GetDeviceOperationByIdAsync(int id)
    {
        try { return await _deviceApiService.GetDeviceOperationByIdAsync(id); }
        catch (Exception ex) { await ShowApiErrorDialog($"getting device operation {id}", ex); return null; }
    }

    /// <summary>
    /// Creates a new device operation for a device card
    /// </summary>
    public async Task<DeviceOperation?> CreateDeviceOperationForCardAsync(DeviceOperation operation, int deviceCardId) 
    {
        try { return await _deviceApiService.CreateDeviceOperationAsync(operation, deviceCardId); }
        catch (Exception ex) { await ShowApiErrorDialog("creating device operation", ex); return null; }
    }

    /// <summary>
    /// Updates an existing device operation
    /// </summary>
    public async Task<bool> UpdateDeviceOperationAsync(DeviceOperation operation)
    {
        try { await _deviceApiService.UpdateDeviceOperationAsync(operation); return true; }
        catch (Exception ex) { await ShowApiErrorDialog($"updating device operation {operation.Id}", ex); return false; }
    }

    /// <summary>
    /// Deletes a device operation by ID
    /// </summary>
    public async Task<bool> DeleteDeviceOperationAsync(int id)
    {
        var confirmResult = await ErrorDialog.ShowWithButtonsAsync($"Are you sure you want to delete operation {id}?", "Confirm Deletion", ButtonEnum.YesNo, Icon.Question);
        if (confirmResult != ButtonResult.Yes) return false;

        try { await _deviceApiService.DeleteDeviceOperationAsync(id); return true; }
        catch (Exception ex) { await ShowApiErrorDialog($"deleting device operation {id}", ex); return false; }
    }
}
