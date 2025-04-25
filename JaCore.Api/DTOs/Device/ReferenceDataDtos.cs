using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.DTOs.Device;

// --- Category DTOs ---

public record CategoryReadDto(Guid Id, string Name, string? Description);

public record CategoryCreateDto(
    [Required] [StringLength(100)] string Name,
    [StringLength(500)] string? Description
);

public record CategoryUpdateDto(
    [Required] [StringLength(100)] string Name,
    [StringLength(500)] string? Description
);

// --- Supplier DTOs ---

public record SupplierReadDto(Guid Id, string Name, string? ContactName, string? Email, string? Phone, string? Address);

public record SupplierCreateDto(
    [Required] [StringLength(150)] string Name,
    [StringLength(100)] string? ContactName,
    [EmailAddress] [StringLength(100)] string? Email,
    [Phone] [StringLength(30)] string? Phone,
    [StringLength(250)] string? Address
);

public record SupplierUpdateDto(
    [Required] [StringLength(150)] string Name,
    [StringLength(100)] string? ContactName,
    [EmailAddress] [StringLength(100)] string? Email,
    [Phone] [StringLength(30)] string? Phone,
    [StringLength(250)] string? Address
);

// --- Service DTOs ---

public record ServiceReadDto(Guid Id, string Name, string? Description, string ProviderName, string? ContactInfo);

public record ServiceCreateDto(
    [Required] [StringLength(100)] string Name,
    [StringLength(500)] string? Description,
    [Required] [StringLength(150)] string ProviderName,
    [StringLength(200)] string? ContactInfo
);

public record ServiceUpdateDto(
    [Required] [StringLength(100)] string Name,
    [StringLength(500)] string? Description,
    [Required] [StringLength(150)] string ProviderName,
    [StringLength(200)] string? ContactInfo
); 