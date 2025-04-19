using JaCore.Api.Interfaces.Services;
using JaCore.Api.Models.Device; // For generic repo registration
using JaCore.Api.Repositories.Common; // For generic repo registration
using JaCore.Api.Repositories.Device;
using JaCore.Api.Services.Device;
using JaCore.Api.Services.User; // Added
using JaCore.Api.Interfaces.Repositories.Device; // For specific repos
using JaCore.Api.Interfaces.Common; // Added for IRepository<>
using JaCore.Api.Services;
using JaCore.Api.Services.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace JaCore.Api.Configuration;

public static class ServicesConfig
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register application services (Device)
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISupplierService, SupplierService>();
        // services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<IDeviceCardService, DeviceCardService>();
        services.AddScoped<IDeviceOperationService, DeviceOperationService>();
        services.AddScoped<IEventService, EventService>();
        
        // Register User service
        services.AddScoped<IUserService, UserService>();

        // Note: IJwtService is already registered in AuthConfig.cs
        // Note: Specific repos like ICategoryRepository are registered in RepositoriesConfig
        
        return services;
    }
} 