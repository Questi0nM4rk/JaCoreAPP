using JaCore.Api.DTOs.Device;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JaCore.Api.Services.Abstractions.Device;

public interface ICategoryService
{
    Task<CategoryReadDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CategoryReadDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<CategoryReadDto?> CreateCategoryAsync(CategoryCreateDto createDto, CancellationToken cancellationToken = default);
    Task<bool> UpdateCategoryAsync(Guid id, CategoryUpdateDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);
} 