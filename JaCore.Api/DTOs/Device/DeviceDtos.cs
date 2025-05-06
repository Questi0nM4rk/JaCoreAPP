using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.DTOs.Device;

// --- Device DTOs ---

public record DeviceDto(
    Guid Id,
    string Name,
    DateTimeOffset? ActivationDate,
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