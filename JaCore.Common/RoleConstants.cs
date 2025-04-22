// filepath: d:\RiderProjects\JaCoreApp\JaCore.Common\RoleConstants.cs
namespace JaCore.Common;

/// <summary>
/// Shared Role constants used across different projects in the JaCore application.
/// </summary>
public static class RoleConstants // Renamed from ApiConstants
{
    /// <summary>
    /// Standard Role names used for authorization.
    /// </summary>
    public static class Roles
    {
        public const string Management = "Management";
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Debug = "Debug";
    }
}