namespace JaCoreUI.Models.Device;

public class Service
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Contact { get; set; }

    public bool HasContact => !string.IsNullOrEmpty(Contact);
    
    public Service() { }

    public Service(Service source)
    {
        Id = source.Id;
        Name = source.Name;
        Contact = source.Contact;
    }
}