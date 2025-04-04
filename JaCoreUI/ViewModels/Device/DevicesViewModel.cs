using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using JaCoreUI.Data;
using JaCoreUI.Factories;
using JaCoreUI.Models.Elements.Device;
using JaCoreUI.Services;

namespace JaCoreUI.ViewModels.Device;

public partial class DevicesViewModel(DeviceService deviceService,
                                        ApiService apiService, CurrentPageService currentPageService)
    : PageViewModel(ApplicationPageNames.Devices, ApplicationPageNames.Devices)
{
    
    // Collection of devices to display in the DataGrid
    [ObservableProperty]
    public partial ObservableCollection<Models.Elements.Device.Device> Devices { get; set; } = apiService.GetDevices(); 


    // Command to handle editing a device
    [RelayCommand]
    private void EditDevice(int id)
    {
        // needs to save the current device before rewriting it
        deviceService.CurrentDevice = Devices[id];
        currentPageService.NavigateTo(ApplicationPageNames.DeviceDetails);
    }

    // Command to handle editing a device
    [RelayCommand]
    private void CreateDev()
    {
        // needs to save the current device before rewriting it
        deviceService.CurrentDevice = new Models.Elements.Device.Device();
        currentPageService.NavigateTo(ApplicationPageNames.DeviceDetails);
    }
    protected override void OnDesignTimeConstructor()
    {
        throw new NotImplementedException();
    }
}