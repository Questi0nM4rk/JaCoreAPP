using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using JaCoreUI.Data;
using JaCoreUI.Services;
using System.Linq;
using JaCoreUI.Models.Device;
using DeviceService = JaCoreUI.Services.Device.DeviceService;

namespace JaCoreUI.ViewModels.Device;

public partial class DeviceDetailsViewModel : PageViewModel
{
    [ObservableProperty] public partial Event? LastCalibration { get; set; }

    [ObservableProperty] public partial Event? LastService { get; set; }
    
    [ObservableProperty] public partial DeviceService DeviceService { get; set; }

    public DeviceDetailsViewModel(DeviceService deviceService) : base(ApplicationPageNames.DeviceDetails,
        ApplicationPageNames.Devices)
    {
        DeviceService = deviceService;

        LastCalibration = GetLastCalibration();
        LastService = GetLastService();
    }

    [RelayCommand]
    private void EditDeviceOperation(int id)
    {
        Console.WriteLine($"Editing device operation with ID: {id}");
        // Implement logic here
    }

    [RelayCommand]
    private void EditEvent(int id)
    {
        Console.WriteLine($"Editing event with ID: {id}");
        // Implement logic here
    }

    [RelayCommand]
    private void AddCard()
    {
        Console.WriteLine("Adding new card");
    }

    public IRelayCommand SaveDeviceCommand => DeviceService.SaveDeviceCommand;

    private Event? GetLastCalibration()
    {
        if (DeviceService.TempDevice == null)
            return null;
        
        if (!DeviceService.TempDevice.HasCard)
            return null;

        if (!DeviceService.TempDevice.DeviceCard!.HasEvents)
            return null;

        var lastCal = DeviceService.TempDevice.DeviceCard!.Events!
            .Where(e => e.Type == EventType.Calibration)
            .OrderByDescending(e => e.From)
            .FirstOrDefault();

        return lastCal;
    }

    private Event? GetLastService()
    {
        if (DeviceService.TempDevice == null)
            return null;
        
        if (!DeviceService.TempDevice.HasCard)
            return null;

        if (!DeviceService.TempDevice.DeviceCard!.HasService)
            return null;

        var lastService = DeviceService.TempDevice.DeviceCard!.Events!
            .Where(e => e.Type == EventType.Service)
            .OrderByDescending(e => e.From)
            .FirstOrDefault();

        return lastService;
    }

    protected override void OnDesignTimeConstructor()
    {
        throw new NotImplementedException();
    }

    public override bool Validate()
    {
        if (DeviceService.TempDevice == null)
            throw new ArgumentNullException(nameof(DeviceService.TempDevice));
        
        if (string.IsNullOrEmpty(DeviceService.TempDevice.Name))
            return false;

        if (DeviceService.TempDevice.DeviceCard == null)
            return true;
        
        if (DeviceService.TempDevice.DeviceCard.SerialNumber == null)
            return false;

        return true;
    }
}