using AutoMapper;
using AutoMapper.QueryableExtensions;
using JaCore.Api.Data;
using JaCore.Api.DTOs.Device; // For Service DTOs
using JaCore.Api.Entities.Device; // For Service entity
using JaCore.Api.Services.Abstractions.Device; // For IServiceService
using JaCore.Api.Services.Repositories.Device; // For IServiceRepository
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JaCore.Api.Services.Device;

// Note: Class name matches the entity/concept, not the interface directly
public class ServiceService : BaseDeviceService, IServiceService
{
    // Renamed repository field to avoid conflict with entity name
    private readonly IServiceRepository _svcRepository;
    private readonly IMapper _mapper;

    public ServiceService(
        IServiceRepository repository,
        ApplicationDbContext context,
        ILogger<ServiceService> logger,
        IMapper mapper) : base(logger, context)
    {
        _svcRepository = repository;
        _mapper = mapper;
    }

    public async Task<ServiceReadDto?> GetServiceByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _svcRepository.GetByIdAsync(id, cancellationToken);
        return _mapper.Map<ServiceReadDto?>(entity);
    }

    public async Task<IEnumerable<ServiceReadDto>> GetAllServicesAsync(CancellationToken cancellationToken = default)
    {
        return await _svcRepository.GetQueryable()
                                .ProjectTo<ServiceReadDto>(_mapper.ConfigurationProvider)
                                .ToListAsync(cancellationToken);
    }

    public async Task<ServiceReadDto?> CreateServiceAsync(ServiceCreateDto createDto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Service>(createDto); // Map to Service entity
        await _svcRepository.AddAsync(entity, cancellationToken);
        bool saved = await SaveChangesAsync(cancellationToken);
        return saved ? _mapper.Map<ServiceReadDto>(entity) : null;
    }

    public async Task<bool> UpdateServiceAsync(Guid id, ServiceUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _svcRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        _mapper.Map(updateDto, entity);
        _svcRepository.Update(entity);
        return await SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteServiceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _svcRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        _svcRepository.Remove(entity);
        return await SaveChangesAsync(cancellationToken);
    }
} 