namespace JaCore.Api.Entities.Device;

/// <summary>
/// Base entity class providing common properties like Id, CreatedAt, ModifiedAt.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
} 