namespace JaCore.Api.Entities.Device;

public class Supplier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }

    // Navigation property for devices supplied by this supplier
    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
} 