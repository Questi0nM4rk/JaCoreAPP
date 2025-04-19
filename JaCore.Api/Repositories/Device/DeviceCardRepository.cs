using JaCore.Api.Data;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Models.Device;
using JaCore.Api.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JaCore.Api.Repositories.Device;

/// <summary>
/// Repository implementation for DeviceCard entities.
/// </summary>
public class DeviceCardRepository : Repository<DeviceCard>, IDeviceCardRepository
{
    private new readonly ApplicationDbContext _context;
    private readonly ILogger<DeviceCardRepository> _logger;

    public DeviceCardRepository(ApplicationDbContext context, ILogger<DeviceCardRepository> logger)
        : base(context) // Changed: Only pass context to base
    {
        _context = context; // Keep this assignment for the 'new' _context field
        _logger = logger;
    }

    /// <summary>
    /// Gets a device card by its associated device ID asynchronously.
    /// </summary>
    public async Task<DeviceCard?> GetByDeviceIdAsync(int deviceId)
    {
        _logger.LogInformation("Attempting to find DeviceCard by DeviceId: {DeviceId}", deviceId);
        try
        {
            // Assuming the Device model has a DeviceCardId linking back
            // Or if DeviceCard model itself has a DeviceId property (less common for 1-to-1)
            // Need to adjust based on actual model structure if different.
            // This query assumes DeviceCard has a navigation property 'Device' with an 'Id'.
            return await _context.DeviceCards
                                 .Include(dc => dc.Device) // Include device to check its Id
                                 .FirstOrDefaultAsync(dc => dc.Device != null && dc.Device.Id == deviceId);
            
            // --- OR --- If Device has the DeviceCardId FK:
            // var device = await _context.Devices.Include(d => d.DeviceCard).FirstOrDefaultAsync(d => d.Id == deviceId);
            // return device?.DeviceCard;

            // --- OR --- If DeviceCard *itself* has a DeviceId FK (less likely for 1:1):
            // return await _context.DeviceCards.FirstOrDefaultAsync(dc => dc.DeviceId == deviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting DeviceCard by DeviceId: {DeviceId}", deviceId);
            throw; // Re-throw the exception to be handled by the service layer
        }
    }

    // Implement other specific methods from IDeviceCardRepository here
    // e.g., override AddAsync/UpdateAsync if specific logic/includes are needed
    
     public override async Task<DeviceCard?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Attempting to find DeviceCard by Id: {Id}", id);
        // Example: Include related entities if needed when getting by ID
        return await _context.DeviceCards
            // .Include(dc => dc.Supplier) // Uncomment if needed
            // .Include(dc => dc.Service)  // Uncomment if needed
            // .Include(dc => dc.Events)   // Uncomment if needed
            .FirstOrDefaultAsync(e => e.Id == id);
    }
} 