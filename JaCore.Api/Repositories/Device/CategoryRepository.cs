using JaCore.Api.Data;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Models.Device;
using JaCore.Api.Repositories.Common;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore; // Needed for AnyAsync

namespace JaCore.Api.Repositories.Device;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    private new readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // Override GetAllAsync to provide default sorting by Name if no specific order is requested.
    public override async Task<IEnumerable<Category>> GetAllAsync(
        int pageNumber = 1, 
        int pageSize = 20, 
        Expression<Func<Category, object>>? orderBy = null, 
        bool ascending = true)
    {
        // Default sort by Name if no specific order is provided
        orderBy ??= c => c.Name;
        
        return await base.GetAllAsync(pageNumber, pageSize, orderBy, ascending);
    }

    public override async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public override async Task<Category> AddAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        
        return category;
    }

    public override async Task<Category> UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        
        return category;
    }

    public override async Task DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task<bool> HasLinkedDevicesAsync(int categoryId)
    {
        // Use the DbContext directly here as it involves querying a different DbSet
        return await _context.Devices.AnyAsync(d => d.CategoryId == categoryId);
    }

    // Add category-specific repository methods here if needed in the future.
} 