using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.DTOs.Device;

// --- Device DTOs ---

public record DeviceReadDto(
    Guid Id,
    string Name,
    string SerialNumber,
    string? ModelNumber,
    string? Manufacturer,
    DateTimeOffset? PurchaseDate,
    DateTimeOffset? WarrantyExpiryDate,
    Guid? CategoryId,
    string? CategoryName, // Include related data
    Guid? SupplierId,
    string? SupplierName, // Include related data
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt
);

public record DeviceCreateDto(
    [Required] [StringLength(100)] string Name,
    [Required] [StringLength(100)] string SerialNumber,
    [StringLength(100)] string? ModelNumber,
    [StringLength(100)] string? Manufacturer,
    DateTimeOffset? PurchaseDate,
    DateTimeOffset? WarrantyExpiryDate,
    Guid? CategoryId, // FK only for creation
    Guid? SupplierId  // FK only for creation
);

public record DeviceUpdateDto(
    [Required] [StringLength(100)] string Name,
    [StringLength(100)] string? ModelNumber,
    [StringLength(100)] string? Manufacturer,
    DateTimeOffset? PurchaseDate,
    DateTimeOffset? WarrantyExpiryDate,
    Guid? CategoryId,
    Guid? SupplierId
    // SerialNumber is usually not updatable, hence omitted
); 