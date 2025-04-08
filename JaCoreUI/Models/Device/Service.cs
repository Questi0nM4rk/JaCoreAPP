namespace JaCoreUI.Models.Device;

public class Service
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public string? Contact { get; set; }

    public bool HasContact => !string.IsNullOrEmpty(Contact);
}