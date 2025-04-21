using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Models.Device;

namespace JaCore.Api.Interfaces.Services;

/// <summary>
/// Interface for managing device categories.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Gets all categories asynchronously with pagination.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>A paginated list of Category DTOs.</returns>
    Task<PaginatedListDto<CategoryDto>> GetAllAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Gets a specific category by ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the category.</param>
    /// <returns>The Category DTO, or null if not found.</returns>
    Task<CategoryDto?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new category asynchronously.
    /// </summary>
    /// <param name="createDto">The DTO containing category data for creation.</param>
    /// <returns>The created Category DTO.</returns>
    Task<CategoryDto> CreateAsync(CreateCategoryDto createDto);

    /// <summary>
    /// Updates an existing category asynchronously.
    /// </summary>
    /// <param name="id">The ID of the category to update.</param>
    /// <param name="updateDto">The DTO containing updated category data.</param>
    /// <returns>True if update was successful, false otherwise (e.g., not found).</returns>
    Task<bool> UpdateAsync(int id, UpdateCategoryDto updateDto);

    /// <summary>
    /// Deletes a category by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the category to delete.</param>
    /// <returns>True if deletion was successful, false otherwise (e.g., not found or validation failed).</returns>
    /// <exception cref="InvalidOperationException">Thrown if category cannot be deleted due to linked devices.</exception>
    Task<bool> DeleteAsync(int id);
} 