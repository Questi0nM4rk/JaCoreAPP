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
        public const string VersionString = "1.0";
        public static readonly ApiVersion Version = new(1, 0);
        // Add other versions here, e.g.:
        // public const string V2_0_String = "2.0";
        // public static readonly ApiVersion V2_0 = new(2, 0);
    }

    /// <summary>
    /// Base route prefix including version placeholder.
    /// </summary>
    private const string RoutePrefixBase = $"api/v{Versions.VersionString}"; // Keep version placeholder for AddApiVersioning

    public static class BasePaths
    {
        public const string Auth = $"{RoutePrefixBase}/auth";
        public const string Users = $"{RoutePrefixBase}/users";
        // Device Module Base Paths
        public const string Devices = $"{RoutePrefixBase}/devices";
        public const string DeviceCards = $"{RoutePrefixBase}/device-cards";
        public const string Categories = $"{RoutePrefixBase}/categories";
        public const string Suppliers = $"{RoutePrefixBase}/suppliers";
        public const string Services = $"{RoutePrefixBase}/services";
        public const string Events = $"{RoutePrefixBase}/events";
        public const string DeviceOperations = $"{RoutePrefixBase}/device-operations";
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
        public const string HealthCheck = $"{RoutePrefixBase}/healthz"; // Keep health check separate
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
        public const string Delete = GetById;
        public const string Update = GetById;
    }

    public static class UserRoutes
    {
        public const string GetAll = BasePaths.Users;
        public const string GetById = $"{BasePaths.Users}/{UserEndpoints.GetById}";
        public const string Update = $"{BasePaths.Users}/{UserEndpoints.Update}";
        public const string Me = $"{BasePaths.Users}/{UserEndpoints.Me}";
        public const string UpdateRoles = $"{BasePaths.Users}/{UserEndpoints.UpdateRoles}";
        public const string Delete = $"{BasePaths.Users}/{UserEndpoints.Delete}"; // Same route for DELETE
    }

    // --- Device Module Routes --- 

    public static class DeviceEndpoints
    {
        public const string GetById = "{id:guid}";
        public const string GetBySerial = "serial/{serialNumber}";
    }
    public static class DeviceRoutes
    {
        public const string GetAll = BasePaths.Devices;
        public const string GetById = $"{BasePaths.Devices}/{DeviceEndpoints.GetById}";
        public const string GetBySerial = $"{BasePaths.Devices}/{DeviceEndpoints.GetBySerial}";
        public const string Create = BasePaths.Devices;
        public const string Update = GetById;
        public const string Delete = GetById;
    }

    public static class DeviceCardEndpoints
    {
        public const string GetById = "{id:guid}";
        public const string GetByDeviceId = "by-device/{deviceId:guid}";
    }
    public static class DeviceCardRoutes
    {
        public const string GetAll = BasePaths.DeviceCards;
        public const string GetById = $"{BasePaths.DeviceCards}/{DeviceCardEndpoints.GetById}";
        public const string GetByDeviceId = $"{BasePaths.DeviceCards}/{DeviceCardEndpoints.GetByDeviceId}";
        public const string Create = BasePaths.DeviceCards;
        public const string Update = GetById;
        public const string Delete = GetById;
    }

    public static class CategoryEndpoints
    {
        public const string GetById = "{id:guid}";
    }
    public static class CategoryRoutes
    {
        public const string GetAll = BasePaths.Categories;
        public const string GetById = $"{BasePaths.Categories}/{CategoryEndpoints.GetById}";
        public const string Create = BasePaths.Categories;
        public const string Update = GetById;
        public const string Delete = GetById;
    }

    public static class SupplierEndpoints
    {
        public const string GetById = "{id:guid}";
    }
    public static class SupplierRoutes
    {
        public const string GetAll = BasePaths.Suppliers;
        public const string GetById = $"{BasePaths.Suppliers}/{SupplierEndpoints.GetById}";
        public const string Create = BasePaths.Suppliers;
        public const string Update = GetById;
        public const string Delete = GetById;
    }

    public static class ServiceEndpoints // Renamed from ServiceEntity
    {
        public const string GetById = "{id:guid}";
    }
    public static class ServiceRoutes // Renamed from ServiceEntity
    {
        public const string GetAll = BasePaths.Services;
        public const string GetById = $"{BasePaths.Services}/{ServiceEndpoints.GetById}";
        public const string Create = BasePaths.Services;
        public const string Update = GetById;
        public const string Delete = GetById;
    }

    public static class EventEndpoints
    {
        public const string GetById = "detail/{id:guid}"; // Specific detail route
        public const string GetByCardId = "by-card/{deviceCardId:guid}";
        public const string DeleteById = "{id:guid}"; // Different endpoint for delete
    }
    public static class EventRoutes
    {
        // No GetAll for Events typically
        public const string GetByCardId = $"{BasePaths.Events}/{EventEndpoints.GetByCardId}";
        public const string GetById = $"{BasePaths.Events}/{EventEndpoints.GetById}";
        public const string Create = BasePaths.Events;
        public const string Delete = $"{BasePaths.Events}/{EventEndpoints.DeleteById}";
    }

    public static class DeviceOperationEndpoints // Renamed from Operation
    {
        public const string GetById = "detail/{id:guid}";
        public const string GetByCardId = "by-card/{deviceCardId:guid}";
        public const string UpdateById = "{id:guid}";
        public const string DeleteById = "{id:guid}";
    }
    public static class DeviceOperationRoutes // Renamed from Operation
    {
        // No GetAll for Operations typically
        public const string GetByCardId = $"{BasePaths.DeviceOperations}/{DeviceOperationEndpoints.GetByCardId}";
        public const string GetById = $"{BasePaths.DeviceOperations}/{DeviceOperationEndpoints.GetById}";
        public const string Create = BasePaths.DeviceOperations;
        public const string Update = $"{BasePaths.DeviceOperations}/{DeviceOperationEndpoints.UpdateById}";
        public const string Delete = $"{BasePaths.DeviceOperations}/{DeviceOperationEndpoints.DeleteById}";
    }

    // --- End Device Module Routes ---

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
        public const string Section = "Jwt"; // Updated section name
        public const string Issuer = $"{Section}:Issuer";
        public const string Audience = $"{Section}:Audience";
        public const string Secret = $"{Section}:Secret";
        public const string AccessExpiryMinutes = $"{Section}:AccessTokenExpirationMinutes"; // Adjusted name
        public const string RefreshExpiryDays = $"{Section}:RefreshTokenExpirationDays"; // Adjusted name
    }

    /// <summary>
    /// Common query parameter names.
    /// </summary>
    public static class QueryParams
    {
        public const string PageNumber = "pageNumber";
        public const string PageSize = "pageSize";
    }
}
