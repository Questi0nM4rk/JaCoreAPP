using System;
using System.Collections.ObjectModel;
using System.Linq;
using JaCoreUI.Models.Elements.Device;

namespace JaCoreUI.Services;

public static class RandomGenerator
{
    private static readonly Random Random = new();
    private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    // Generates a random 12-character string
    public static string GenerateRandomString(int length = 12)
    {
        var chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = Characters[Random.Next(Characters.Length)];
        }
        return new string(chars);
    }

    // Generates random DateTime within specified range (default: last 10 years)
    public static DateTime GenerateRandomDateTime(DateTime? start = null, DateTime? end = null)
    {
        var startDate = start ?? DateTime.Now.AddYears(-10);
        var endDate = end ?? DateTime.Now;

        if (startDate > endDate)
            throw new ArgumentException("Start date must be before end date");

        TimeSpan timeSpan = endDate - startDate;
        var randomSeconds = Random.Next(0, (int)timeSpan.TotalSeconds);
        
        return startDate.AddSeconds(randomSeconds);
    }
}

public class ApiService
{

    public ObservableCollection<Service> GetServices()
    {
        return
        [
            new Service()
            {
                Name = "TondaOpravy Sro.",
            }
        ];
    }

    public ObservableCollection<Supplier> GetSuppliers()
    {
        return 
        [
            new Supplier()
            {
                Name = "Tony Sro.",
            }
        ];
    }
    
    public ObservableCollection<Device> GetDevices()
    {
        // dummy load devices
        ObservableCollection<Device> devices = [];
        for (var i = 0; i <= 13; i++)
        {
            var device = new Device
            {
                Id = i,
                Name = $"Device {i}",
                Category = $"Category {(char)('A' + i - 1)}",
                DeviceCard = i % 2 != 0 ? new DeviceCard()
                {
                    SerialNumber = RandomGenerator.GenerateRandomString(),
                    DateOfActivation = RandomGenerator.GenerateRandomDateTime(),
                    Supplier = new Supplier()
                    {
                        Name = $"Supplier {i}",
                        Contact = i % 2 != 0 ? $"Contact {i}" : null
                    },
                    Events = [
                        new Event
                        {
                            Id = 0,
                            Type = EventType.Operation,
                            Who = "User",
                            Description = "Navazit chemikalie",
                        },
                        new Event
                        {
                            Id = 1,
                            Type = EventType.Malfunction,
                            Who = "System",
                        },
                        new Event
                        {
                            Id = 2,
                            Type = EventType.Maintenance,
                            Who = "Administrator",
                        },
                        new Event
                        {
                            Id = 3,
                            Type = EventType.Operation,
                            Who = "User",
                            Description = "Zmerit PH",
                        }
                    ]
                } : null,
                DeviceOperations = CreateDeviceOperations(i)
            };
                
            devices.Add(device);
        }
        
        return devices;
    }
    
    private ObservableCollection<DeviceOperation> CreateDeviceOperations(int deviceId)
    {
        return new ObservableCollection<DeviceOperation>
        {
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
        };
    }
}