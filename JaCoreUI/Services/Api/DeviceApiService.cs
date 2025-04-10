using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Models.Device;
using JaCoreUI.Models.UI;
using DeviceCard = JaCoreUI.Models.Device.DeviceCard;
using DeviceOperation = JaCoreUI.Models.Device.DeviceOperation;

namespace JaCoreUI.Services.Api;

public static class RandomGenerator
{
    private static readonly Random Random = new();
    private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    // Generates a random 12-character string
    public static string GenerateRandomString(int length = 12)
    {
        var chars = new char[length];
        for (var i = 0; i < length; i++) chars[i] = Characters[Random.Next(Characters.Length)];

        return new string(chars);
    }

    // Generates random DateTime within specified range (default: last 10 years)
    public static DateTime GenerateRandomDateTime(DateTime? start = null, DateTime? end = null)
    {
        var startDate = start ?? DateTime.Now.AddYears(-10);
        var endDate = end ?? DateTime.Now;

        if (startDate > endDate)
            throw new ArgumentException("Start date must be before end date");

        var timeSpan = endDate - startDate;
        var randomSeconds = Random.Next(0, (int)timeSpan.TotalSeconds);

        return startDate.AddSeconds(randomSeconds);
    }
}

public partial class DeviceApiService : ObservableObject
{
    private readonly ObservableCollection<Service> _services;
    private readonly ObservableCollection<Supplier> _suppliers;
    private readonly ObservableCollection<Category> _categories;
    private readonly ObservableCollection<Models.Device.Device> _devices;

    public DeviceApiService()
    {
        _services = CreateServices();
        _suppliers = CreateSuppliers();
        _categories = CreateCategories();
        _devices = CreateDevices();
    }

    public ObservableCollection<Service> GetServices(int page = 0, int count = 10)
    {
        return new ObservableCollection<Service>(_services.Skip(page * count).Take(count));
    }

    public ObservableCollection<Supplier> GetSuppliers(int page = 0, int count = 10)
    {
        return new ObservableCollection<Supplier>(_suppliers.Skip(page * count).Take(count));
    }

    public ObservableCollection<Category> GetCategories(int page = 0, int count = 10)
    {
        return new ObservableCollection<Category>(_categories.Skip(page * count).Take(count));
    }

    public ObservableCollection<Models.Device.Device> GetDevices(int page = 0, int count = 10)
    {
        return new ObservableCollection<Models.Device.Device>(_devices.Skip(page * count).Take(count));
    }

    private ObservableCollection<Service> CreateServices()
    {
        var services = new ObservableCollection<Service>();

        for (var i = 0; i <= 20; i++)
            services.Add(new Service
            {
                Id = i,
                Name = $"Service {i}",
                Contact = i % 2 == 0 ? $"Contact {i}" : null
            });

        return services;
    }

    private ObservableCollection<Supplier> CreateSuppliers()
    {
        var suppliers = new ObservableCollection<Supplier>();

        for (var i = 0; i <= 20; i++)
            suppliers.Add(new Supplier
            {
                Id = i,
                Name = $"Supplier {i}",
                Contact = i % 2 == 0 ? $"Contact {i}" : null
            });

        return suppliers;
    }

    private ObservableCollection<Category> CreateCategories()
    {
        var categories = new ObservableCollection<Category>();

        for (var i = 1; i <= 20; i++)
            categories.Add(new Category()
            {
                Id = i,
                Name = $"Category {i}"
            });

        return categories;
    }

    private ObservableCollection<Models.Device.Device> CreateDevices()
    {
        // dummy load devices
        ObservableCollection<Models.Device.Device> devices = [];
        for (var i = 0; i <= 50; i++)
        {
            var device = new Models.Device.Device
            {
                Id = i,
                Name = $"Device {i}",
                Category = _categories[i % 4],
                DeviceCard = i % 2 != 0
                    ? new DeviceCard()
                    {
                        SerialNumber = RandomGenerator.GenerateRandomString(),
                        DateOfActivation = RandomGenerator.GenerateRandomDateTime(),
                        Supplier = _suppliers[i % 3],
                        Service = _services[i % 3],
                        Events =
                        [
                            new Event
                            {
                                Id = 0,
                                Type = EventType.Operation,
                                Who = "User",
                                Description = "Navazit chemikalie"
                            },
                            new Event
                            {
                                Id = 1,
                                Type = EventType.Malfunction,
                                Who = "System",
                                Description = "Nepovedlo se nacist data"
                            },
                            new Event
                            {
                                Id = 2,
                                Type = EventType.Maintenance,
                                Who = "Administrator",
                                Description = "Vse v poradku, kontrola probehla hladce"
                            },
                            new Event
                            {
                                Id = 3,
                                Type = EventType.Operation,
                                Who = "User",
                                Description = "Zmerit PH"
                            }
                        ]
                    }
                    : null,
                DeviceOperations = CreateDeviceOperations(i)
            };

            devices.Add(device);
        }

        return devices;
    }

    private ObservableCollection<DeviceOperation> CreateDeviceOperations(int deviceId)
    {
        return
        [
            new DeviceOperation
            {
                DeviceId = deviceId,
                Name = $"Operation 1 for Device {deviceId}",
                IsRequired = true,
                OrderIndex = 0
            },

            new DeviceOperation
            {
                DeviceId = deviceId,
                Name = $"Operation 2 for Device {deviceId}",
                IsRequired = false,
                OrderIndex = 1
            },

            new DeviceOperation
            {
                DeviceId = deviceId,
                Name = $"Operation 3 for Device {deviceId}",
                IsRequired = false,
                OrderIndex = 2
            }
        ];
    }
    
    // ==============================================

    public async Task<bool> UpdateDevice(Models.Device.Device device)
    {
        var remDev = _devices.FirstOrDefault(x => x.Id == device.Id);
       
        if (remDev == null)
        {
            await ErrorDialog.ShowWithButtonsAsync(message: "No device found for deletion");
            return false;
        }
               
        _devices.Remove(remDev);
        _devices.Add(device);
        return true;
    }
    
    public async Task<bool> DeleteDevice(int deviceId)
    {
        var device = _devices.FirstOrDefault(x => x.Id == deviceId);

        if (device == null)
        {
            await ErrorDialog.ShowWithButtonsAsync(message: "No device found for deletion");
            return false;
        }
        
        _devices.Remove(device);
        return true;
    }
    
    public async Task<Models.Device.Device?> GetDevice(int deviceId)
    {
        var device = _devices.FirstOrDefault(d => d.Id == deviceId);

        // Further in, make it check the DB for the device with that ID
        return device ?? null;
    }

    public async Task<Models.Device.Device> NewDevice()
    {
        // Further into the app, this needs to check with the API / DB for the index, so some call that will send new device request to the db and returns it.

        return new Models.Device.Device()
        {
            Id = _devices.Count,
            Name = null,
            IsCompleted = false,
            CreatedAt = DateTime.Now,
        };
    }
    
    
}