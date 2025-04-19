// filepath: d:\RiderProjects\JaCoreApp\JaCore.Common\ApiConstants.cs
namespace JaCore.Common;

/// <summary>
/// Contains constants related to the API endpoints and route patterns
/// </summary>
public static class ApiConstants
{
    /// <summary>
    /// Base route prefix for all API routes
    /// </summary>
    public const string ApiRoutePrefix = "api";

    /// <summary>
    /// Version prefix for API routes, following the base prefix (e.g., api/v1)
    /// </summary>
    public const string ApiVersionPrefix = "v1";

    /// <summary>
    /// Full base route for API endpoints, combining prefix and version
    /// </summary>
    public const string BaseRoute = ApiRoutePrefix + "/" + ApiVersionPrefix;
    
    /// <summary>
    /// Route pattern templates for standard API operations
    /// </summary>
    public static class Routes
    {
        /// <summary>
        /// Template for GetAll operation - returns a collection
        /// </summary>
        public const string GetAll = "";
        
        /// <summary>
        /// Template for GetById operation - requires an ID parameter
        /// </summary>
        public const string GetById = "{id}";
        
        /// <summary>
        /// Template for Create operation - typically POST with no ID
        /// </summary>
        public const string Create = "";
        
        /// <summary>
        /// Template for Update operation - requires an ID parameter
        /// </summary>
        public const string Update = "{id}";
        
        /// <summary>
        /// Template for Delete operation - requires an ID parameter
        /// </summary>
        public const string Delete = "{id}";
    }
    
    /// <summary>
    /// Authorization policies for API endpoints
    /// </summary>
    public static class Policies
    {
        /// <summary>
        /// Policy for read-only access to resources
        /// </summary>
        public const string ReadOnly = "ReadOnly";
        
        /// <summary>
        /// Policy for read-write access to resources
        /// </summary>
        public const string ReadWrite = "ReadWrite";
        
        /// <summary>
        /// Policy for full access including delete operations
        /// </summary>
        public const string FullAccess = "FullAccess";
        
        /// <summary>
        /// Policy for admin-only access
        /// </summary>
        public const string AdminOnly = "AdminOnly";
        
        /// <summary>
        /// Policy for system operations (debugging, monitoring)
        /// </summary>
        public const string SystemOperations = "SystemOperations";
    }
    
    /// <summary>
    /// User roles in the application
    /// </summary>
    public static class Roles
    {
        /// <summary>
        /// Regular user role with basic permissions
        /// </summary>
        public const string User = "User";
        
        /// <summary>
        /// Management role with elevated permissions
        /// </summary>
        public const string Management = "Management";
        
        /// <summary>
        /// Administrator role with full system access
        /// </summary>
        public const string Admin = "Admin";
        
        /// <summary>
        /// Debug role for system diagnostics
        /// </summary>
        public const string Debug = "Debug";
    }
}