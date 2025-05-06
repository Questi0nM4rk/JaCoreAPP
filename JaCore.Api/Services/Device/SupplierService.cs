using AutoMapper;
using AutoMapper.QueryableExtensions;
using JaCore.Api.Data;
using JaCore.Api.DTOs.Device; // For Supplier DTOs
using JaCore.Api.Entities.Device; // For Supplier entity
using JaCore.Api.Services.Abstractions.Device; // For ISupplierService
using JaCore.Api.Services.Repositories.Device; // For ISupplierRepository
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JaCore.Api.Services.Device;

public class SupplierService : BaseDeviceService, ISupplierService
{
    private readonly ISupplierRepository _repository;
    private readonly IMapper _mapper;

    public SupplierService(
        ISupplierRepository repository,
        ApplicationDbContext context,
        ILogger<SupplierService> logger,
        IMapper mapper) : base(logger, context)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<SupplierReadDto?> GetSupplierByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return _mapper.Map<SupplierReadDto?>(entity);
    }

    public async Task<IEnumerable<SupplierReadDto>> GetAllSuppliersAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetQueryable()
                                .ProjectTo<SupplierReadDto>(_mapper.ConfigurationProvider)
                                .ToListAsync(cancellationToken);
    }

    public async Task<SupplierReadDto?> CreateSupplierAsync(SupplierCreateDto createDto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Supplier>(createDto);
        await _repository.AddAsync(entity, cancellationToken);
        bool saved = await SaveChangesAsync(cancellationToken);
        return saved ? _mapper.Map<SupplierReadDto>(entity) : null;
    }

    public async Task<bool> UpdateSupplierAsync(Guid id, SupplierUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        _mapper.Map(updateDto, entity);
        _repository.Update(entity);
        return await SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteSupplierAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        _repository.Remove(entity);
        return await SaveChangesAsync(cancellationToken);
    }
} 