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
    /// Usage: [Route(ApiConstants.Routing.ApiRoutePrefix + "/your-controller")]
    /// </summary>
    public const string ApiRoutePrefix = "api/v{version:apiVersion}";

    /// <summary>
    /// Standard relative route segments for common actions.
    /// Usage: [HttpGet(ApiConstants.Routes.GetById)]
    /// </summary>
    public static class Routes
    {
        // No need for GetAll = "" if using controller root
        public const string GetById = "{id:guid}";
        // Create = "" (usually handled by HttpPost on controller root)
        // Update = "{id:guid}" (usually handled by HttpPut on controller root)
        // Delete = "{id:guid}" (usually handled by HttpDelete on controller root)

        // Specific actions
        public const string Register = "register";
        public const string Login = "login";
        public const string Refresh = "refresh";
        public const string Logout = "logout";
        public const string GetCurrentUser = "me";
        public const string AdminOnlyData = "admin-only";
        public const string UpdateRoles = "{id:guid}/roles";
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
