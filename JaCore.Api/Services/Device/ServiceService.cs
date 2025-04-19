using AutoMapper;
using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Interfaces.Services;
using JaCore.Api.Models.Device;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JaCore.Api.Services.Device;

public class ServiceService : IServiceService
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ILogger<ServiceService> _logger;

    public ServiceService(IServiceRepository serviceRepository, ILogger<ServiceService> logger)
    {
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    public async Task<PaginatedListDto<ServiceDto>> GetAllAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;

        var totalCount = await _serviceRepository.CountAsync();
        var services = await _serviceRepository.GetAllAsync(pageNumber, pageSize);
        var serviceDtos = services.Select(s => new ServiceDto { Id = s.Id, Name = s.Name, Contact = s.Contact });
        
        return new PaginatedListDto<ServiceDto>(serviceDtos, totalCount, pageNumber, pageSize);
    }

    public async Task<ServiceDto?> GetByIdAsync(int id)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        return service == null ? null : new ServiceDto { Id = service.Id, Name = service.Name, Contact = service.Contact };
    }

    public async Task<ServiceDto> CreateAsync(CreateServiceDto createDto)
    {
        if (string.IsNullOrWhiteSpace(createDto.Name) || createDto.Name.Length > 100)
        {
            throw new ArgumentException("Service name is required and cannot exceed 100 characters.", nameof(createDto.Name));
        }

        var service = new Service { Name = createDto.Name, Contact = createDto.Contact };
        var addedService = await _serviceRepository.AddAsync(service);
        return new ServiceDto { Id = addedService.Id, Name = addedService.Name, Contact = addedService.Contact };
    }

    public async Task<bool> UpdateAsync(int id, UpdateServiceDto updateDto)
    {
        var existingService = await _serviceRepository.GetByIdAsync(id);
        if (existingService == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(updateDto.Name) || updateDto.Name.Length > 100)
        {
            throw new ArgumentException("Service name is required and cannot exceed 100 characters.", nameof(updateDto.Name));
        }

        existingService.Name = updateDto.Name;
        existingService.Contact = updateDto.Contact;

        try
        {
            await _serviceRepository.UpdateAsync(existingService);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict occurred while updating service {ServiceId}", id);
            if (!await _serviceRepository.ExistsAsync(id))
            {
                return false;
            }
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        if (service == null)
        {
            return false;
        }

        // Add checks for linked entities (e.g., DeviceCards) if necessary
        // Example: var linkedCards = await _context.DeviceCards.AnyAsync(dc => dc.ServiceId == id);
        // if (linkedCards) { throw new InvalidOperationException(...); }

        try
        {
            await _serviceRepository.DeleteAsync(id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting service {ServiceId}", id);
            return false;
        }
    }
} 