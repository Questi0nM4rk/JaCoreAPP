using JaCore.Common.Device;

namespace JaCore.Api.Entities.Device;

public class Device : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty; // Assuming unique per device
    public string? ModelNumber { get; set; }
    public string? Manufacturer { get; set; }
    public DateTimeOffset? PurchaseDate { get; set; }
    public DateTimeOffset? WarrantyExpiryDate { get; set; }

    // Foreign Key for Category
    public Guid? CategoryId { get; set; }
    // Navigation Property for Category
    public virtual Category? Category { get; set; }

    // Foreign Key for Supplier
    public Guid? SupplierId { get; set; }
    // Navigation Property for Supplier
    public virtual Supplier? Supplier { get; set; }

    // Navigation Property for the associated DeviceCard (One-to-One)
    // Configuration in DbContext might be needed if DeviceCardId isn't explicitly in DeviceCard
    public virtual DeviceCard? DeviceCard { get; set; }
} 