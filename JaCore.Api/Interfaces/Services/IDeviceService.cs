using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;

namespace JaCore.Api.Interfaces.Services;

public interface IDeviceService
{
    Task<PaginatedListDto<DeviceDto>> GetAllAsync(int pageNumber, int pageSize);
    Task<DeviceDto?> GetByIdAsync(int id);
    Task<DeviceDto> CreateAsync(CreateDeviceDto createDto);
    Task<bool> UpdateAsync(int id, UpdateDeviceDto updateDto);
    Task<bool> DeleteAsync(int id);
} 