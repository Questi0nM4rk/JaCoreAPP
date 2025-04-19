using JaCore.Api.Data;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Models.Device;
using JaCore.Api.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JaCore.Api.Dtos.Common; // For PaginatedList

namespace JaCore.Api.Repositories.Device;

/// <summary>
/// Repository implementation for Event entities.
/// </summary>
public class EventRepository : Repository<Event>, IEventRepository
{
    private new readonly ApplicationDbContext _context;
    private readonly ILogger<EventRepository> _logger;

    public EventRepository(ApplicationDbContext context, ILogger<EventRepository> logger)
        : base(context)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all events associated with a specific DeviceCard asynchronously.
    /// </summary>
    public async Task<JaCore.Api.Dtos.Common.PaginatedListDto<Event>> GetByDeviceCardIdAsync(int deviceCardId, int pageNumber, int pageSize)
    {
        _logger.LogInformation("Getting Events for DeviceCardId: {DeviceCardId}, Page: {PageNumber}, Size: {PageSize}", 
            deviceCardId, pageNumber, pageSize);
            
        var query = _context.Events
                            .Where(ev => ev.DeviceCardId == deviceCardId)
                            .OrderByDescending(ev => ev.From); // Example sorting by date

        try
        {
            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedListDto<Event>(items, totalCount, pageNumber, pageSize);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error getting Events for DeviceCardId: {DeviceCardId}", deviceCardId);
             throw;
        }
    }

    // Override base methods like AddAsync, UpdateAsync, DeleteAsync if specific logic needed
    public override async Task<Event?> GetByIdAsync(int id)
    {
        // Example: Include DeviceCard if needed when getting by ID
        return await _context.Events
            // .Include(ev => ev.DeviceCard) // Uncomment if needed
            .FirstOrDefaultAsync(ev => ev.Id == id);
    }
} 