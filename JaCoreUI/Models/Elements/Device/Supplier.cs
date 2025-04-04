namespace JaCoreUI.Models.Elements.Device;

public class Supplier
{
    public required string Name { get; set; }
    public string? Contact { get; set; }
    
    public bool HasContact => !string.IsNullOrEmpty(Contact);
}