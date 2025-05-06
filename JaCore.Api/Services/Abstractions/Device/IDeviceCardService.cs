using JaCore.Api.DTOs.Device;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JaCore.Api.Services.Abstractions.Device;

public interface IDeviceCardService
{
    // Methods defined based on DeviceCardsController usage
    Task<IEnumerable<DeviceCardReadDto>> GetAllDeviceCardsAsync(CancellationToken cancellationToken = default);
    Task<DeviceCardReadDto?> GetDeviceCardByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DeviceCardReadDto>> GetDeviceCardsByDeviceIdAsync(Guid deviceId, CancellationToken cancellationToken = default);
    Task<DeviceCardReadDto?> CreateDeviceCardAsync(DeviceCardCreateDto createDto, CancellationToken cancellationToken = default);
    Task<bool> UpdateDeviceCardAsync(Guid id, DeviceCardUpdateDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteDeviceCardAsync(Guid id, CancellationToken cancellationToken = default);
}
