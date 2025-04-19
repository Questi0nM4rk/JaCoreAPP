using JaCore.Api.Data;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Repositories.Common;
using System.Linq.Expressions;
// Note: Fully qualify Device model to avoid ambiguity with namespace
using DeviceModel = JaCore.Api.Models.Device.Device;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JaCore.Api.Repositories.Device;

public class DeviceRepository : Repository<DeviceModel>, IDeviceRepository
{
    private new readonly ApplicationDbContext _context;
    private readonly ILogger<DeviceRepository> _logger;

    public DeviceRepository(ApplicationDbContext context, ILogger<DeviceRepository> logger) 
        : base(context)
    {
        _context = context;
        _logger = logger;
    }

    // Override AddAsync to set timestamps
    public override async Task<DeviceModel> AddAsync(DeviceModel device)
    {
        device.CreatedAt = DateTimeOffset.UtcNow;
        device.ModifiedAt = device.CreatedAt; // Set ModifiedAt on creation as well
        return await base.AddAsync(device);
    }

    // Override UpdateAsync to set ModifiedAt timestamp
    public override async Task<DeviceModel> UpdateAsync(DeviceModel device)
    {
        // Ensure the entity is tracked before updating the timestamp
        var trackedEntity = await _dbSet.FindAsync(device.Id);
        if (trackedEntity != null)
        {
            // Update properties on the tracked entity
            _context.Entry(trackedEntity).CurrentValues.SetValues(device);
            trackedEntity.ModifiedAt = DateTimeOffset.UtcNow;
            // Detach the incoming entity to avoid tracking conflicts if necessary
            _context.Entry(device).State = EntityState.Detached;
            return await base.UpdateAsync(trackedEntity); // Update the tracked entity
        }
        _logger.LogWarning("Attempted to update non-tracked or non-existent Device entity with Id {DeviceId}. Attaching and setting state.", device.Id);
        // Fallback if not tracked (less ideal)
        device.ModifiedAt = DateTimeOffset.UtcNow;
        return await base.UpdateAsync(device);
    }
    
    // Override GetAllAsync to provide default sorting by ModifiedAt descending
    public override async Task<IEnumerable<DeviceModel>> GetAllAsync(
        int pageNumber = 1, 
        int pageSize = 20, 
        Expression<Func<DeviceModel, object>>? orderBy = null, 
        bool ascending = true)
    {
        // Default sort by ModifiedAt descending if no specific order is provided
        if (orderBy == null)
        {
            orderBy = d => d.ModifiedAt ?? DateTimeOffset.MinValue; // Handle potential null
            ascending = false; // Default to descending for ModifiedAt
        }
        
        return await base.GetAllAsync(pageNumber, pageSize, orderBy, ascending);
    }

    public override async Task<DeviceModel?> GetByIdAsync(int id)
    {
        return await _context.Devices
            // .Include(d => d.Category) // Example include
            // .Include(d => d.DeviceCard) // Example include
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public override async Task DeleteAsync(int id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device != null)
        {
            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Gets a device by its associated DeviceCard ID asynchronously.
    /// </summary>
    public async Task<DeviceModel?> GetByDeviceCardIdAsync(int deviceCardId)
    {
        _logger.LogInformation("Attempting to find Device by DeviceCardId: {DeviceCardId}", deviceCardId);
        try
        {
            return await _context.Devices
                                 .FirstOrDefaultAsync(d => d.DeviceCardId == deviceCardId);
        }
        catch (Exception ex)
        {            
            _logger.LogError(ex, "Error getting Device by DeviceCardId: {DeviceCardId}", deviceCardId);
            throw;
        }
    }

    // Add device-specific repository methods here if needed.
} 