using Microsoft.AspNetCore.Mvc; // Required for ApiVersion

namespace JaCore.Api.Helpers;

/// <summary>
/// Constants specific to the JaCore.Api project.
/// </summary>
public static class ApiConstants
{
    /// <summary>
    /// API Version constants.
    /// </summary>
    public static class Versions
    {
        public const string V1_0_String = "1.0";
        public static readonly ApiVersion V1_0 = new(1, 0);
        // Add other versions here, e.g.:
        // public const string V2_0_String = "2.0";
        // public static readonly ApiVersion V2_0 = new(2, 0);
    }

    /// <summary>
    /// Base route prefix including version placeholder.
    /// Usage: [Route(ApiConstants.Routing.RoutePrefix + "/your-controller")]
    /// </summary>
    public const string RoutePrefix = $"api/v{Versions.V1_0_String}";

    public static class BasePaths
    {
        public const string Auth = $"{RoutePrefix}/auth";
        public const string Users = $"{RoutePrefix}/users";
    }

    // Authentication and Authorization related constants
    public static class AuthEndpoints
    {
        public const string Register = "register";
        public const string Login = "login";
        public const string Refresh = "refresh";
        public const string Logout = "logout";
        public const string AdminOnlyData = "admin-only";
    }

    public static class AuthRoutes
    {
        public const string HealthCheck = $"{RoutePrefix}/healthz";
        public const string Register = $"{BasePaths.Auth}/{AuthEndpoints.Register}";
        public const string Login = $"{BasePaths.Auth}/{AuthEndpoints.Login}";
        public const string Refresh = $"{BasePaths.Auth}/{AuthEndpoints.Refresh}";
        public const string Logout = $"{BasePaths.Auth}/{AuthEndpoints.Logout}";
        public const string AdminOnly = $"{BasePaths.Auth}/{AuthEndpoints.AdminOnlyData}";
    }

    // User management related constants
    public static class UserEndpoints
    {
        public const string GetById = "{id:guid}";
        public const string UpdateRoles = $"{GetById}/roles";
        public const string Me = "me";
    }

    public static class UserRoutes
    {
        public const string GetAll = $"{BasePaths.Users}";
        public const string GetById = $"{BasePaths.Users}/{UserEndpoints.GetById}";
        public const string Update = $"{BasePaths.Users}/{UserEndpoints.GetById}";
        public const string Me = $"{BasePaths.Users}/{UserEndpoints.Me}";
        public const string UpdateRoles = $"{BasePaths.Users}/{UserEndpoints.UpdateRoles}";
        public const string Delete = $"{BasePaths.Users}/{UserEndpoints.GetById}";
    }

    /// <summary>
    /// Authorization policy names used within the API.
    /// </summary>
    public static class Policies
    {
        public const string AdminOnly = "AdminOnly";
        // Add other policies as needed
    }

    /// <summary>
    /// Configuration keys for JWT settings.
    /// </summary>
    public static class JwtConfigKeys
    {
        public const string Section = "Jwt"; // Optional: Base section name
        public const string Issuer = $"{Section}:Issuer";
        public const string Audience = $"{Section}:Audience";
        public const string Secret = $"{Section}:Secret";
        public const string AccessExpiryMinutes = $"{Section}:AccessExpiryMinutes";
        public const string RefreshExpiryDays = $"{Section}:RefreshExpiryDays";
    }
}
