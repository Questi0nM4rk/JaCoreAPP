using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;

namespace JaCore.Api.Interfaces.Services;

public interface ISupplierService
{
    Task<PaginatedListDto<SupplierDto>> GetAllAsync(int pageNumber, int pageSize);
    Task<SupplierDto?> GetByIdAsync(int id);
    Task<SupplierDto> CreateAsync(CreateSupplierDto createDto);
    Task<bool> UpdateAsync(int id, UpdateSupplierDto updateDto);
    Task<bool> DeleteAsync(int id);
} 