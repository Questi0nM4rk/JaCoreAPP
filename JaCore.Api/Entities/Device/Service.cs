namespace JaCore.Api.Entities.Device;

public class Service : BaseEntity // Renamed from ServiceEntity to avoid conflict
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string? ContactInfo { get; set; }

    // Navigation Property for potential link to Devices or DeviceCards if needed later
    // Example: public virtual ICollection<DeviceCard> ServicedDeviceCards { get; set; } = new List<DeviceCard>();
} 