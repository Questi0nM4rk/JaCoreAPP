using JaCore.Api.DTOs.Device;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JaCore.Api.Services.Abstractions.Device;

public interface ISupplierService
{
    Task<SupplierReadDto?> GetSupplierByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SupplierReadDto>> GetAllSuppliersAsync(CancellationToken cancellationToken = default);
    Task<SupplierReadDto?> CreateSupplierAsync(SupplierCreateDto createDto, CancellationToken cancellationToken = default);
    Task<bool> UpdateSupplierAsync(Guid id, SupplierUpdateDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteSupplierAsync(Guid id, CancellationToken cancellationToken = default);
} 