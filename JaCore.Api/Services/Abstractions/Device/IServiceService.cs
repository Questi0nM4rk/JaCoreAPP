using JaCore.Api.DTOs.Device;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JaCore.Api.Services.Abstractions.Device;

// Renamed from IServiceEntityService
public interface IServiceService 
{
    Task<ServiceReadDto?> GetServiceByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceReadDto>> GetAllServicesAsync(CancellationToken cancellationToken = default);
    Task<ServiceReadDto?> CreateServiceAsync(ServiceCreateDto createDto, CancellationToken cancellationToken = default);
    Task<bool> UpdateServiceAsync(Guid id, ServiceUpdateDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteServiceAsync(Guid id, CancellationToken cancellationToken = default);
} 