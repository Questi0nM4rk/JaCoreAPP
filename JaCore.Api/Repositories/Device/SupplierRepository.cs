using JaCore.Api.Data;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Models.Device;
using JaCore.Api.Repositories.Common;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace JaCore.Api.Repositories.Device;

public class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    private new readonly ApplicationDbContext _context;

    public SupplierRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // Override GetAllAsync to provide default sorting by Name if no specific order is requested.
    public override async Task<IEnumerable<Supplier>> GetAllAsync(
        int pageNumber = 1, 
        int pageSize = 20, 
        Expression<Func<Supplier, object>>? orderBy = null, 
        bool ascending = true)
    {
        // Default sort by Name if no specific order is provided
        orderBy ??= s => s.Name;
        
        return await base.GetAllAsync(pageNumber, pageSize, orderBy, ascending);
    }

    public override async Task<Supplier?> GetByIdAsync(int id)
    {
        return await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public override async Task<Supplier> AddAsync(Supplier supplier)
    {
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();
        
        return supplier;
    }

    public override async Task<Supplier> UpdateAsync(Supplier supplier)
    {
        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync();
        return supplier;
    }

    public override async Task DeleteAsync(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier != null)
        {
            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
        }
    }

    // Add supplier-specific repository methods here if needed.
} 