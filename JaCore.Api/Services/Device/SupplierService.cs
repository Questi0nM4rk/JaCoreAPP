using AutoMapper;
using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Interfaces.Services;
using JaCore.Api.Models.Device;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JaCore.Api.Services.Device;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly ILogger<SupplierService> _logger;
    // Inject DbContext if specific cross-entity checks needed (none apparent for Supplier yet)

    public SupplierService(ISupplierRepository supplierRepository, ILogger<SupplierService> logger)
    {
        _supplierRepository = supplierRepository;
        _logger = logger;
    }

    public async Task<PaginatedListDto<SupplierDto>> GetAllAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;

        var totalCount = await _supplierRepository.CountAsync();
        var suppliers = await _supplierRepository.GetAllAsync(pageNumber, pageSize);
        var supplierDtos = suppliers.Select(s => new SupplierDto { Id = s.Id, Name = s.Name, Contact = s.Contact });
        
        return new PaginatedListDto<SupplierDto>(supplierDtos, totalCount, pageNumber, pageSize);
    }

    public async Task<SupplierDto?> GetByIdAsync(int id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        return supplier == null ? null : new SupplierDto { Id = supplier.Id, Name = supplier.Name, Contact = supplier.Contact };
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierDto createDto)
    {
        if (string.IsNullOrWhiteSpace(createDto.Name) || createDto.Name.Length > 100)
        {
            throw new ArgumentException("Supplier name is required and cannot exceed 100 characters.", nameof(createDto.Name));
        }
        
        var supplier = new Supplier { Name = createDto.Name, Contact = createDto.Contact };
        var addedSupplier = await _supplierRepository.AddAsync(supplier);
        return new SupplierDto { Id = addedSupplier.Id, Name = addedSupplier.Name, Contact = addedSupplier.Contact };
    }

    public async Task<bool> UpdateAsync(int id, UpdateSupplierDto updateDto)
    {
        var existingSupplier = await _supplierRepository.GetByIdAsync(id);
        if (existingSupplier == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(updateDto.Name) || updateDto.Name.Length > 100)
        {
            throw new ArgumentException("Supplier name is required and cannot exceed 100 characters.", nameof(updateDto.Name));
        }

        existingSupplier.Name = updateDto.Name;
        existingSupplier.Contact = updateDto.Contact;

        try
        {
            await _supplierRepository.UpdateAsync(existingSupplier);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict occurred while updating supplier {SupplierId}", id);
            if (!await _supplierRepository.ExistsAsync(id))
            {
                 return false;
            }
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null)
        {
            return false;
        }

        // Add checks for linked entities (e.g., DeviceCards) if necessary, potentially injecting DbContext
        // Example: var linkedCards = await _context.DeviceCards.AnyAsync(dc => dc.SupplierId == id);
        // if (linkedCards) { throw new InvalidOperationException(...); }

        try
        {
            await _supplierRepository.DeleteAsync(id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier {SupplierId}", id);
            return false;
        }
    }
} 