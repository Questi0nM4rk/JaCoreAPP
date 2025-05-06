using JaCore.Api.DTOs.Device;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JaCore.Api.Services.Abstractions.Device;

public interface IDeviceOperationService
{
    // Methods defined based on DeviceOperationsController usage
    Task<IEnumerable<DeviceOperationReadDto>> GetOperationsByDeviceCardIdAsync(Guid deviceCardId, CancellationToken cancellationToken = default);
    Task<DeviceOperationReadDto?> GetOperationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DeviceOperationReadDto?> CreateOperationAsync(DeviceOperationCreateDto createDto, CancellationToken cancellationToken = default);
    Task<bool> UpdateOperationAsync(Guid id, DeviceOperationUpdateDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteOperationAsync(Guid id, CancellationToken cancellationToken = default);
} 