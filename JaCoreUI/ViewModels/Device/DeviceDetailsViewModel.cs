using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using JaCoreUI.Data;
using JaCoreUI.Services;
using System.Linq;
using JaCoreUI.Models.Device;
using DeviceService = JaCoreUI.Services.Device.DeviceService;
using JaCore.Common.Device;
using System.ComponentModel;

namespace JaCoreUI.ViewModels.Device;

public partial class DeviceDetailsViewModel : PageViewModel
{
    [ObservableProperty] private Event? _lastCalibration;
    [ObservableProperty] private Event? _lastService;
    [ObservableProperty] private DeviceService _deviceService;
    [ObservableProperty] private bool _isLoading = false;
    [ObservableProperty] private bool _isSaving = false;

    // Add collections for related data managed by the ViewModel
    [ObservableProperty]
    private ObservableCollection<DeviceOperation> _deviceOperations = new();

    [ObservableProperty]
    private ObservableCollection<Event> _events = new();

    public DeviceDetailsViewModel(DeviceService deviceService) : base(ApplicationPageNames.DeviceDetails,
        ApplicationPageNames.Devices)
    {
        _deviceService = deviceService;
        _deviceService.PropertyChanged += DeviceService_PropertyChanged;
        
        // Initial load when the ViewModel is created
        LoadRelatedDataAsync();
    }
    
    private async void DeviceService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DeviceService.TempDevice))
        {
            // TempDevice has changed, reload related data
            await LoadRelatedDataAsync();
        }
    }

    private async Task LoadRelatedDataAsync()
    {
        IsLoading = true;
        DeviceOperations.Clear();
        Events.Clear();

        if (DeviceService.TempDevice == null)
        {
            RefreshEventData(); // Refresh last service/calib (will be null)
            IsLoading = false;
            return;
        }

        // Load Device Operations
        if (DeviceService.TempDevice.HasOperations) // Check if there are IDs to load
        {
            // Assuming GetDeviceOperationsForDeviceAsync fetches based on DeviceId
            // We might need to adjust this if operations are tied to DeviceCardId
            try
            {
                var ops = await DeviceService.GetDeviceOperationsForDeviceAsync(DeviceService.TempDevice.Id);
                foreach (var op in ops)
                {
                    DeviceOperations.Add(op);
                }
            }
            catch (Exception ex)
            {
                 // Use the service's error dialog or log locally
                 Console.WriteLine($"Error loading device operations: {ex.Message}"); 
                 // Consider showing error to user via a property binding or notification
            }
        }

        // Load Events if Device Card exists
        if (DeviceService.TempDevice.HasCard && DeviceService.TempDevice.DeviceCard!.HasEvents)
        {
             try
             {
                 // We need a method in DeviceService/DeviceApiService to get events by card ID
                 // Let's assume a method GetEventsForDeviceCardAsync exists
                 var events = await DeviceService.GetEventsForDeviceCardAsync(DeviceService.TempDevice.DeviceCard.Id);
                 foreach (var ev in events)
                 {
                     Events.Add(ev);
                 }
             }
             catch (Exception ex)
             {
                  Console.WriteLine($"Error loading events: {ex.Message}");
                  // Handle error display
             }
        }
        
        RefreshEventData(); // Refresh last service/calib based on newly loaded events
        IsLoading = false;
    }

    private void RefreshEventData()
    {
        LastCalibration = GetLastCalibration();
        LastService = GetLastService();
    }

    [RelayCommand]
    private async Task EditDeviceOperation(int id)
    {
        Console.WriteLine($"Editing device operation with ID: {id}");
        // Implement logic here using our API service
        await Task.Delay(1); // Add minimal await to make it truly async
    }

    [RelayCommand]
    private async Task EditEvent(int id)
    {
        Console.WriteLine($"Editing event with ID: {id}");
        // Implement logic here using our API service
        await Task.Delay(1); // Add minimal await to make it truly async
    }

    [RelayCommand]
    private async Task AddCard()
    {
        Console.WriteLine("Adding new card");
        // Implement logic here using our API service
        await Task.Delay(1); // Add minimal await to make it truly async
    }
    
    [RelayCommand]
    private async Task SaveDevice()
    {
        if (!Validate()) return;
        
        try
        {
            IsSaving = true;
            await DeviceService.SaveDeviceCommand.ExecuteAsync(null);
            // After saving, we might want to navigate back or refresh data
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving device: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private Event? GetLastCalibration()
    {
        if (!Events.Any())
            return null;

        var lastCal = Events
            .Where(e => e.Type == EventType.Calibration)
            .OrderByDescending(e => e.From)
            .FirstOrDefault();
        return lastCal;
    }

    private Event? GetLastService()
    {
         if (!Events.Any())
            return null;

        var lastService = Events
            .Where(e => e.Type == EventType.Service)
            .OrderByDescending(e => e.From)
            .FirstOrDefault();
        return lastService;
    }

    protected override void OnDesignTimeConstructor()
    {
        DeviceService = new DeviceService(new Services.Api.DeviceApiService(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build()));
        // Add a dummy device for design time
        DeviceService.TempDevice = DeviceService.CreateDeviceTemplate();
    }

    public override bool Validate()
    {
        if (DeviceService.TempDevice == null)
            return false;
        
        if (string.IsNullOrEmpty(DeviceService.TempDevice.Name))
            return false;

        if (DeviceService.TempDevice.DeviceCard == null)
            return true;
        
        if (DeviceService.TempDevice.DeviceCard.SerialNumber == null)
            return false;

        return true;
    }
}