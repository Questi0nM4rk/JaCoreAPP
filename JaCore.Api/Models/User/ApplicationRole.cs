using Microsoft.AspNetCore.Identity;

namespace JaCore.Api.Models.User;

/// <summary>
/// Application role model extending IdentityRole
/// </summary>
public class ApplicationRole : IdentityRole
{
    /// <summary>
    /// Description of the role
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public ApplicationRole() : base()
    {
    }
    
    /// <summary>
    /// Constructor with role name
    /// </summary>
    /// <param name="roleName">Name of the role</param>
    public ApplicationRole(string roleName) : base(roleName)
    {
    }
    
    /// <summary>
    /// Constructor with role name and description
    /// </summary>
    /// <param name="roleName">Name of the role</param>
    /// <param name="description">Description of the role</param>
    public ApplicationRole(string roleName, string description) : base(roleName)
    {
        Description = description;
    }
} 