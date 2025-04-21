namespace JaCore.Api.Dtos.Device;

// DTO for returning category data
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// DTO for creating a new category
public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
}

// DTO for updating an existing category
public class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
} 