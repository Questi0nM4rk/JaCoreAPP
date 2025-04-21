using JaCore.Api.Interfaces.Common;
using JaCore.Api.Models.Device;

namespace JaCore.Api.Interfaces.Repositories.Device;

/// <summary>
/// Interface for the repository managing Category entities.
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    // Add Category-specific methods here if needed, e.g.:
    Task<bool> HasLinkedDevicesAsync(int categoryId);
} 