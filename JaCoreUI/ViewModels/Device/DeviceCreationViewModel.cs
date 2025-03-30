using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using JaCoreUI.Data;
using JaCoreUI.Models.Elements.Device;
using JaCoreUI.Services;

namespace JaCoreUI.ViewModels.Device;

public partial class DeviceCreationViewModel : PageViewModel
{
    private readonly DeviceService _deviceService;
    
    [ObservableProperty]
    public partial Models.Elements.Device.Device CurrentDevice { get; set; }

    [ObservableProperty]
    public partial string? SelectedCategory { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<string> Categories { get; set; } =
    [
        "Category 1",
        "Category 2",
        "Category 3",
        "Category 4",
        "Category 5"
    ];

    [ObservableProperty]
    public partial ObservableCollection<DeviceOperation> DeviceOperations { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<Event> Events { get; set; } = new();

    public DeviceCreationViewModel(DeviceService deviceService) : base(ApplicationPageNames.DeviceCreation)
    {
        _deviceService = deviceService;
        CurrentDevice = _deviceService.CurrentDevice ?? throw new NullReferenceException();
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
        // Implement logic here
    }

    protected override void OnDesignTimeConstructor()
    {
        throw new NotImplementedException();
    }
}
