using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Data;
using JaCoreUI.Models.Device;
using JaCoreUI.Services.Api;

namespace JaCoreUI.Services.Device;

public partial class DeviceService : ObservableObject
{
    private readonly Navigation.CurrentPageService _currentPageService;
    private readonly DeviceApiService _deviceApiService;
    public Models.Device.Device? CurrentDevice { get; set; }

    [ObservableProperty] public partial ObservableCollection<Service> Services { get; set; }

    [ObservableProperty] public partial ObservableCollection<Supplier> Suppliers { get; set; }

    [ObservableProperty] public partial ObservableCollection<Category> Categories { get; set; }

    [ObservableProperty] public partial ObservableCollection<Models.Device.Device> Devices { get; set; }

    public DeviceService(DeviceApiService deviceApiService, Navigation.CurrentPageService currentPageService)
    {
        Devices = deviceApiService.GetDevices();
        Services = deviceApiService.GetServices();
        Suppliers = deviceApiService.GetSuppliers();
        Categories = deviceApiService.GetCategories();
        
        _currentPageService = currentPageService;
        _deviceApiService = deviceApiService;
    }
    
    
    [RelayCommand]
    private void DeviceDetails(int id)
    {
        var device = Devices.FirstOrDefault(p => p.Id == id);

        CurrentDevice = device ?? throw new ArgumentNullException(nameof(device));
        _currentPageService.NavigateTo(ApplicationPageNames.DeviceDetails);
    }
        
    [RelayCommand]
    private void NewDevice(int id)
    {
        CurrentDevice = _deviceApiService.NewDevice();
        Devices.Add(CurrentDevice);
        _currentPageService.NavigateTo(ApplicationPageNames.DeviceDetails);
    }
}