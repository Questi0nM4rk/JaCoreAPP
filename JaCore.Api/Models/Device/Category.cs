using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.Models.Device;

public class Category
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    // Navigation property
    public ICollection<Device> Devices { get; set; } = new List<Device>();
} 