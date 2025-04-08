using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using JaCoreUI.Data;
using JaCoreUI.Factories;
using JaCoreUI.Services;
using DeviceService = JaCoreUI.Services.Device.DeviceService;

namespace JaCoreUI.ViewModels.Device;

public partial class DevicesViewModel : PageViewModel
{
    [ObservableProperty] public partial DeviceService DeviceService { get; set; }

    public DevicesViewModel(DeviceService deviceService) : base(ApplicationPageNames.Devices,
        ApplicationPageNames.Devices)
    {
        DeviceService = deviceService;
    }

    public IRelayCommand NewDeviceCommand => DeviceService.NewDeviceCommand;
    public IRelayCommand DeviceDetailsCommand => DeviceService.DeviceDetailsCommand;

protected override void OnDesignTimeConstructor()
    {
        throw new NotImplementedException();
    }
}