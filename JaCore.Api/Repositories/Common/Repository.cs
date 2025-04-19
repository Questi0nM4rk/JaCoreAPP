using JaCore.Api.Data;
using JaCore.Api.Interfaces.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace JaCore.Api.Repositories.Common;

/// <summary>
/// Generic repository implementation for common CRUD operations.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> GetAllAsync(
        int pageNumber = 1, 
        int pageSize = 20, 
        Expression<Func<T, object>>? orderBy = null, 
        bool ascending = true)
    {
        // Ensure pageNumber is at least 1
        if (pageNumber < 1) pageNumber = 1;
        // Ensure pageSize is positive, or default if invalid
        if (pageSize <= 0) pageSize = 20;

        IQueryable<T> query = _dbSet;

        if (orderBy != null)
        {
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        }

        // Apply pagination
        // Ensure Skip count is non-negative
        int itemsToSkip = (pageNumber - 1) * pageSize;
        query = query.Skip(itemsToSkip).Take(pageSize);

        return await query.ToListAsync();
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        // FindAsync is optimized for finding by primary key
        return await _dbSet.FindAsync(id);
    }

    /// <inheritdoc />
    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <inheritdoc />
    public virtual async Task<T> UpdateAsync(T entity)
    {
        // Attach the entity if it's not tracked, then set state to Modified
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
        // Consider throwing KeyNotFoundException if entity is null and deletion is expected to succeed?
        // For now, just does nothing if not found.
    }

    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(int id)
    {
        // Check if an entity with the given ID exists
        // Need to dynamically build the expression to compare the primary key
        // This assumes the primary key is named 'Id' and is an int.
        // A more robust solution might require a base entity interface or reflection.
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Property(parameter, "Id");
        var constant = Expression.Constant(id);
        var equality = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(equality, parameter);

        return await _dbSet.AnyAsync(lambda);
    }
    
    /// <inheritdoc />
    public virtual async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }
} 