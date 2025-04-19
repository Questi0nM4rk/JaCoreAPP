using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using JaCoreUI.Data;
using JaCoreUI.Factories;
using JaCoreUI.Services;
using DeviceService = JaCoreUI.Services.Device.DeviceService;

namespace JaCoreUI.ViewModels.Device;

public partial class DevicesViewModel : PageViewModel
{
    [ObservableProperty] private DeviceService _deviceService;
    [ObservableProperty] private int _currentPage = 0;
    [ObservableProperty] private int _pageSize = 10;
    [ObservableProperty] private bool _isLoading = false;
    [ObservableProperty] private string _searchQuery = string.Empty;

    public DevicesViewModel(DeviceService deviceService) : base(ApplicationPageNames.Devices,
        ApplicationPageNames.Devices)
    {
        _deviceService = deviceService;
        Task.Run(LoadDevicesAsync);
    }

    private async Task LoadDevicesAsync()
    {
        try
        {
            IsLoading = true;
            await Task.Delay(1);
            // Data is automatically loaded in the DeviceService constructor
            // We could load more data here if needed
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading devices: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadNextPage()
    {
        CurrentPage++;
        await LoadPageAsync(CurrentPage, PageSize);
    }

    [RelayCommand]
    private async Task LoadPreviousPage()
    {
        if (CurrentPage > 0)
        {
            CurrentPage--;
            await LoadPageAsync(CurrentPage, PageSize);
        }
    }

    private async Task LoadPageAsync(int page, int pageSize)
    {
        try
        {
            IsLoading = true;
            var devices = await DeviceService.GetDevicesAsync(page, pageSize);
            // Update the Devices collection in the DeviceService
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                DeviceService.Devices.Clear();
                foreach (var device in devices)
                {
                    DeviceService.Devices.Add(device);
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading page: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Search()
    {
        // In a real implementation, we would add search functionality
        // For now, just reload the first page
        CurrentPage = 0;
        await LoadPageAsync(CurrentPage, PageSize);
    }

    public IRelayCommand NewDeviceCommand => DeviceService.CreateNewDeviceCommand;
    public IRelayCommand DeviceDetailsCommand => DeviceService.OpenDeviceCommand;

    protected override void OnDesignTimeConstructor()
    {
        DeviceService = new DeviceService(new Services.Api.DeviceApiService(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build()));
    }
}