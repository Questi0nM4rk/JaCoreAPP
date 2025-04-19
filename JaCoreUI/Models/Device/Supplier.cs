namespace JaCoreUI.Models.Device;

public class Supplier
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Contact { get; set; }

    public bool HasContact => !string.IsNullOrEmpty(Contact);
    
    public Supplier() { }

    public Supplier(Supplier supplier)
    {
        Id = supplier.Id;
        Name = supplier.Name;
        Contact = supplier.Contact;
    }
}