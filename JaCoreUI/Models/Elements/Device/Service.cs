namespace JaCoreUI.Models.Elements.Device;

public class Service
{
    public required string Name { get; set; }
    public string? Contact { get; set; }
    
    public bool HasContact => !string.IsNullOrEmpty(Contact);
}