using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Force.DeepCloner;
using JaCoreUI.Data;
using JaCoreUI.Models.Device;
using JaCoreUI.Models.UI;
using JaCoreUI.Services.Api;
using MsBox.Avalonia.Enums;

namespace JaCoreUI.Services.Device;

public partial class DeviceService : ObservableObject
{
    private readonly Navigation.CurrentPageService _currentPageService;
    private readonly DeviceApiService _deviceApiService;
    
    // Fix TempDevice to be a proper observable property
    [ObservableProperty]
    public partial Models.Device.Device? TempDevice { get; set; }

    [ObservableProperty] 
    public partial ObservableCollection<Service> Services { get; set; } = new();

    [ObservableProperty] 
    public partial ObservableCollection<Supplier> Suppliers { get; set; } = new();

    [ObservableProperty] 
    public partial ObservableCollection<Category> Categories { get; set; } = new();

    [ObservableProperty] 
    public partial ObservableCollection<Models.Device.Device> Devices { get; set; } = new();

    public DeviceService(DeviceApiService deviceApiService, Navigation.CurrentPageService currentPageService)
    {
        _currentPageService = currentPageService;
        _deviceApiService = deviceApiService;
        
        // Initialize collections
        Devices = deviceApiService.GetDevices();
        Services = deviceApiService.GetServices();
        Suppliers = deviceApiService.GetSuppliers();
        Categories = deviceApiService.GetCategories();
    }

    [RelayCommand]
    private async Task SaveDevice()
    {
        try
        {
            if (TempDevice == null)
                throw new Exception("TempDevice is null");
            
            var device = await _deviceApiService.GetDevice(TempDevice.Id);

            if (device == null)
                throw new Exception("Device not found in database");

            await _deviceApiService.UpdateDevice(TempDevice); 
            
            TempDevice = null;
            
            await ErrorDialog.ShowWithButtonsAsync("Zařízení bylo úspěšně uloženo", "Info", ButtonEnum.Ok, Icon.Info);
        }
        
        catch (Exception ex)
        {
            await ErrorDialog.ShowWithButtonsAsync($"Error saving device: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private async Task DeviceDetails(int id)
    {
        try
        {
            var device = await _deviceApiService.GetDevice(id);
            
            if (device == null)
                throw new Exception("Device not found in database");

            TempDevice = device.DeepClone();
            await _currentPageService.NavigateTo(ApplicationPageNames.DeviceDetails);
        }
        catch (Exception ex)
        {
            await ErrorDialog.ShowWithButtonsAsync($"Error opening device details: {ex.Message}");
        }
    }
        
    [RelayCommand]
    private async Task NewDevice(int id)
    {
        try
        {
            TempDevice = await _deviceApiService.NewDevice();
            await _currentPageService.NavigateTo(ApplicationPageNames.DeviceDetails);
        }
        catch (Exception ex)
        {
            await ErrorDialog.ShowWithButtonsAsync($"Error creating new device: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private async Task DiscardChanges()
    {
        try
        {
            if (TempDevice == null) 
                throw new Exception("TempDevice is null, cannot discard changes");
            
            await _deviceApiService.DeleteDevice(TempDevice.Id);
            TempDevice = null;
            await _currentPageService.NavigateTo(ApplicationPageNames.Devices);
        }
        
        catch (Exception ex)
        {
            await ErrorDialog.ShowWithButtonsAsync($"Error creating new device: {ex.Message}");
        }
       
    }
}
