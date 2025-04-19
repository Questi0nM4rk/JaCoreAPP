namespace JaCore.Api.Models.User;

/// <summary>
/// Defines the available roles in the application
/// </summary>
public static class UserRoles
{
    /// <summary>
    /// Admin role with full access to all features including deletes
    /// </summary>
    public const string Admin = "Admin";
    
    /// <summary>
    /// Debug role with same permissions as Admin but different UI elements
    /// </summary>
    public const string Debug = "Debug";
    
    /// <summary>
    /// Management role with access to most features except deletes
    /// </summary>
    public const string Management = "Management";
    
    /// <summary>
    /// Regular user role with limited access
    /// </summary>
    public const string User = "User";
    
    /// <summary>
    /// All roles that can perform delete operations
    /// </summary>
    public static readonly string[] DeleteRoles = { Admin, Debug };
    
    /// <summary>
    /// All roles that can perform write operations (create/update)
    /// </summary>
    public static readonly string[] WriteRoles = { Admin, Debug, Management };
    
    /// <summary>
    /// All roles that can perform read operations (get/list)
    /// </summary>
    public static readonly string[] ReadRoles = { Admin, Debug, Management, User };
} 