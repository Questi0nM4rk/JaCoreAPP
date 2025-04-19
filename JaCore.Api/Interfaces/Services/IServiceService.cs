using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;

namespace JaCore.Api.Interfaces.Services;

public interface IServiceService // Naming is slightly awkward, consider IServiceManagementService?
{
    Task<PaginatedListDto<ServiceDto>> GetAllAsync(int pageNumber, int pageSize);
    Task<ServiceDto?> GetByIdAsync(int id);
    Task<ServiceDto> CreateAsync(CreateServiceDto createDto);
    Task<bool> UpdateAsync(int id, UpdateServiceDto updateDto);
    Task<bool> DeleteAsync(int id);
} 