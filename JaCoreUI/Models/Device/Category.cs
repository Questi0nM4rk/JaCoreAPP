using System;

namespace JaCoreUI.Models.Device;

public class Category
{
    public int Id { get; set; }
    public string? Name { get; set; }
    
    public Category() { }
    
    public Category(Category source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        
        Id = source.Id;
        Name = source.Name;
    }
    
    public Category Clone() => new Category(this);
}