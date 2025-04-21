using JaCore.Api.Data;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Models.Device;
using JaCore.Api.Repositories.Common;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace JaCore.Api.Repositories.Device;

public class ServiceRepository : Repository<Service>, IServiceRepository
{
    private new readonly ApplicationDbContext _context;

    public ServiceRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // Override GetAllAsync to provide default sorting by Name if no specific order is requested.
    public override async Task<IEnumerable<Service>> GetAllAsync(
        int pageNumber = 1, 
        int pageSize = 20, 
        Expression<Func<Service, object>>? orderBy = null, 
        bool ascending = true)
    {
        // Default sort by Name if no specific order is provided
        orderBy ??= s => s.Name;

        return await base.GetAllAsync(pageNumber, pageSize, orderBy, ascending);
    }

    public override async Task<Service?> GetByIdAsync(int id)
    {
        return await _context.Services
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public override async Task<Service> AddAsync(Service service)
    {
        _context.Services.Add(service);
        await _context.SaveChangesAsync();
        
        return service;
    }

    public override async Task<Service> UpdateAsync(Service service)
    {
        _context.Services.Update(service);
        await _context.SaveChangesAsync();
        return service;
    }

    public override async Task DeleteAsync(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service != null)
        {
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
        }
    }

    // Add service-specific repository methods here if needed.
} 