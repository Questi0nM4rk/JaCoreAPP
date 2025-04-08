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
    [ObservableProperty] public partial Models.Device.Device CurrentDevice { get; set; }

    [ObservableProperty] public partial ObservableCollection<Category> Categories { get; set; }

    [ObservableProperty] public partial Event? LastCalibration { get; set; }

    [ObservableProperty] public partial Event? LastService { get; set; }
    
    [ObservableProperty] public partial DeviceService DeviceService { get; set; }

    public DeviceDetailsViewModel(DeviceService deviceService) : base(ApplicationPageNames.DeviceDetails,
        ApplicationPageNames.Devices)
    {
        CurrentDevice = deviceService.CurrentDevice ?? throw new NullReferenceException();
        Categories = deviceService.Categories;
        
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

    private Event? GetLastCalibration()
    {
        if (!CurrentDevice.HasCard)
            return null;

        if (!CurrentDevice.DeviceCard!.HasEvents)
            return null;

        var lastCal = CurrentDevice.DeviceCard!.Events!
            .Where(e => e.Type == EventType.Calibration)
            .OrderByDescending(e => e.From)
            .FirstOrDefault();

        return lastCal;
    }

    private Event? GetLastService()
    {
        if (!CurrentDevice.HasCard)
            return null;

        if (!CurrentDevice.DeviceCard!.HasService)
            return null;

        var lastService = CurrentDevice.DeviceCard!.Events!
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
        return !string.IsNullOrEmpty(CurrentDevice.Name);
    }
}