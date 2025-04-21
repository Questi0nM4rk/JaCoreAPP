namespace JaCore.Api.Dtos.Device;

// DTO for returning service data
public class ServiceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Contact { get; set; }
}

// DTO for creating a new service
public class CreateServiceDto
{
    public string Name { get; set; } = string.Empty;
    public string? Contact { get; set; }
}

// DTO for updating an existing service
public class UpdateServiceDto
{
    public string Name { get; set; } = string.Empty;
    public string? Contact { get; set; }
} 