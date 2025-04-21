using AutoMapper;
using JaCore.Api.Data;
using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Interfaces.Services;
using JaCore.Api.Models.Device;
using Microsoft.EntityFrameworkCore; // Needed for AnyAsync on DbContext
using Microsoft.Extensions.Logging;

namespace JaCore.Api.Services.Device;

/// <summary>
/// Service for managing device categories.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    // private readonly ApplicationDbContext _context; // No longer needed directly
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ICategoryRepository categoryRepository, /* ApplicationDbContext context, */ ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        // _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedListDto<CategoryDto>> GetAllAsync(int pageNumber, int pageSize)
    {
        // Ensure pageNumber and pageSize are valid (optional, can also be done in controller or base class)
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20; // Or a configurable default
        
        var totalCount = await _categoryRepository.CountAsync();
        var categories = await _categoryRepository.GetAllAsync(pageNumber, pageSize);
        
        // Simple mapping for this DTO
        var categoryDtos = categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name });
        
        return new PaginatedListDto<CategoryDto>(categoryDtos, totalCount, pageNumber, pageSize);
    }

    /// <inheritdoc />
    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category == null ? null : new CategoryDto { Id = category.Id, Name = category.Name };
    }

    /// <inheritdoc />
    public async Task<CategoryDto> CreateAsync(CreateCategoryDto createDto)
    {
        // Basic validation (could be enhanced with FluentValidation)
        if (string.IsNullOrWhiteSpace(createDto.Name) || createDto.Name.Length > 100)
        {
            // Consider throwing a specific validation exception
            throw new ArgumentException("Category name is required and cannot exceed 100 characters.", nameof(createDto.Name));
        }
        
        var category = new Category { Name = createDto.Name };
        var addedCategory = await _categoryRepository.AddAsync(category);
        return new CategoryDto { Id = addedCategory.Id, Name = addedCategory.Name };
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(int id, UpdateCategoryDto updateDto)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(id);
        if (existingCategory == null)
        {
            return false; // Category not found
        }

        // Basic validation
        if (string.IsNullOrWhiteSpace(updateDto.Name) || updateDto.Name.Length > 100)
        {
            throw new ArgumentException("Category name is required and cannot exceed 100 characters.", nameof(updateDto.Name));
        }

        existingCategory.Name = updateDto.Name;

        try
        {
            await _categoryRepository.UpdateAsync(existingCategory);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict occurred while updating category {CategoryId}", id);
            // Check if it was deleted concurrently
            if (!await _categoryRepository.ExistsAsync(id))
            {
                 return false; // Not found (deleted concurrently)
            }
            throw; // Re-throw if it wasn't a deletion conflict
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return false; // Not found
        }

        // Business Rule: Check if any devices are linked to this category using the repository method
        var hasLinkedDevices = await _categoryRepository.HasLinkedDevicesAsync(id);
        if (hasLinkedDevices)
        {
            _logger.LogWarning("Attempted to delete category {CategoryId} which has linked devices.", id);
            throw new InvalidOperationException("Cannot delete category because it has linked devices.");
        }

        try
        {
            await _categoryRepository.DeleteAsync(id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return false; 
        }
    }
} 