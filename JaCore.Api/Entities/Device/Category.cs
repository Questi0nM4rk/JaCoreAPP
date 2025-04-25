namespace JaCore.Api.Entities.Device;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation property for devices belonging to this category
    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
} 