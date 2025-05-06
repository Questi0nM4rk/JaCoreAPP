using JaCore.Api.DTOs.Device;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// Namespace updated to reflect new location
namespace JaCore.Api.Services.Abstractions.Device;

public interface IDeviceService
{
    Task<DeviceDto?> GetDeviceByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DeviceDto>> GetAllDevicesAsync(CancellationToken cancellationToken = default);
    Task<DeviceDto?> CreateDeviceAsync(DeviceCreateDto createDto, CancellationToken cancellationToken = default);
    Task<bool> UpdateDeviceAsync(Guid id, DeviceUpdateDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteDeviceAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DeviceDto?> GetDeviceBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default);
} 