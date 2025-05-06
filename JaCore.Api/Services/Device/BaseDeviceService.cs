using JaCore.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JaCore.Api.Services.Device;

public abstract class BaseDeviceService
{
    protected readonly ILogger _logger;
    protected readonly ApplicationDbContext _context; // For SaveChangesAsync

    protected BaseDeviceService(ILogger logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    protected async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            int result = await _context.SaveChangesAsync(cancellationToken);
            if (result <= 0)
            {
                _logger.LogWarning("No changes were saved to the database.");
                return false;
            }
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database update error occurred while saving changes.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generic error saving changes to the database.");
            return false;
        }
    }
} 