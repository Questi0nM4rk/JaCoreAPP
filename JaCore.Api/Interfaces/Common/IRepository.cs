using System.Linq.Expressions;

namespace JaCore.Api.Interfaces.Common;

/// <summary>
/// Generic repository interface for common CRUD operations.
/// Assumes entities have an integer Id property.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets all entities asynchronously with pagination and optional sorting.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="orderBy">Optional expression to order by.</param>
    /// <param name="ascending">Optional flag for ascending order (defaults to true).</param>
    /// <returns>A collection of entities.</returns>
    Task<IEnumerable<T>> GetAllAsync(
        int pageNumber = 1, 
        int pageSize = 20, 
        Expression<Func<T, object>>? orderBy = null, 
        bool ascending = true);

    /// <summary>
    /// Gets an entity by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <returns>The entity, or null if not found.</returns>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Adds a new entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>The added entity.</returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Updates an existing entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>The updated entity.</returns>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    /// Deletes an entity by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    Task DeleteAsync(int id);
    
    /// <summary>
    /// Checks if an entity exists by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the entity to check.</param>
    /// <returns>True if the entity exists, false otherwise.</returns>
    Task<bool> ExistsAsync(int id);
    
    /// <summary>
    /// Counts the total number of entities asynchronously.
    /// </summary>
    /// <returns>The total count of entities.</returns>
    Task<int> CountAsync();
} 