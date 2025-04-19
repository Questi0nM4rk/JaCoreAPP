using JaCore.Api.Data;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Models.Device;
using JaCore.Api.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JaCore.Api.Dtos.Common; // Needed for PaginatedList return type

namespace JaCore.Api.Repositories.Device;

/// <summary>
/// Repository implementation for DeviceOperation entities.
/// </summary>
public class DeviceOperationRepository : Repository<DeviceOperation>, IDeviceOperationRepository
{
    private new readonly ApplicationDbContext _context;
    private readonly ILogger<DeviceOperationRepository> _logger;

    public DeviceOperationRepository(ApplicationDbContext context, ILogger<DeviceOperationRepository> logger)
        : base(context)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all operations associated with a specific DeviceCard asynchronously.
    /// </summary>
    public async Task<JaCore.Api.Dtos.Common.PaginatedListDto<DeviceOperation>> GetByDeviceCardIdAsync(int deviceCardId, int pageNumber, int pageSize)
    {
        _logger.LogInformation("Getting DeviceOperations for DeviceCardId: {DeviceCardId}, Page: {PageNumber}, Size: {PageSize}", 
            deviceCardId, pageNumber, pageSize);
            
        var query = _context.DeviceOperations
                            .Where(op => op.DeviceCardId == deviceCardId)
                            .OrderBy(op => op.OrderIndex); // Example sorting

        try
        {
            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedListDto<DeviceOperation>(items, totalCount, pageNumber, pageSize);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error getting DeviceOperations for DeviceCardId: {DeviceCardId}", deviceCardId);
             throw;
        }
    }

    // Override base methods like AddAsync, UpdateAsync, DeleteAsync if needed
    // e.g., to include specific related entities or perform extra checks.
    public override async Task<DeviceOperation?> GetByIdAsync(int id)
    {
        // Example: Possibly include DeviceCard when getting by ID
        return await _context.DeviceOperations
            // .Include(op => op.DeviceCard) // Uncomment if needed
            .FirstOrDefaultAsync(op => op.Id == id);
    }
} 