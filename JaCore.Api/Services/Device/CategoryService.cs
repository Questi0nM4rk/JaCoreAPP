using AutoMapper;
using AutoMapper.QueryableExtensions;
using JaCore.Api.Data;
using JaCore.Api.DTOs.Device; // For Category DTOs
using JaCore.Api.Entities.Device; // For Category entity
using JaCore.Api.Services.Abstractions.Device; // For ICategoryService
using JaCore.Api.Services.Repositories.Device; // For ICategoryRepository
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JaCore.Api.Services.Device;

public class CategoryService : BaseDeviceService, ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly IMapper _mapper;

    public CategoryService(
        ICategoryRepository repository,
        ApplicationDbContext context,
        ILogger<CategoryService> logger,
        IMapper mapper) : base(logger, context)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CategoryReadDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return _mapper.Map<CategoryReadDto?>(entity);
    }

    public async Task<IEnumerable<CategoryReadDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        // Using GetQueryable for potential projection
        return await _repository.GetQueryable()
                                .ProjectTo<CategoryReadDto>(_mapper.ConfigurationProvider)
                                .ToListAsync(cancellationToken);
    }

    public async Task<CategoryReadDto?> CreateCategoryAsync(CategoryCreateDto createDto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Category>(createDto);
        await _repository.AddAsync(entity, cancellationToken);
        bool saved = await SaveChangesAsync(cancellationToken);
        return saved ? _mapper.Map<CategoryReadDto>(entity) : null;
    }

    public async Task<bool> UpdateCategoryAsync(Guid id, CategoryUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        _mapper.Map(updateDto, entity);
        _repository.Update(entity);
        return await SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        _repository.Remove(entity);
        return await SaveChangesAsync(cancellationToken);
    }
}

// NOTE: Requires a BaseCrudService implementation similar to other services 