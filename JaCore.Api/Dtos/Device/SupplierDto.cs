namespace JaCore.Api.Dtos.Device;

// DTO for returning supplier data
public class SupplierDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Contact { get; set; }
}

// DTO for creating a new supplier
public class CreateSupplierDto
{
    public string Name { get; set; } = string.Empty;
    public string? Contact { get; set; }
}

// DTO for updating an existing supplier
public class UpdateSupplierDto
{
    public string Name { get; set; } = string.Empty;
    public string? Contact { get; set; }
} 