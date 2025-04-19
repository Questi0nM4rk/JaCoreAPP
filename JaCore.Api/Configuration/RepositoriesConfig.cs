using JaCore.Api.Interfaces;
// using JaCore.Api.Interfaces.Device; // Removed to resolve ambiguity
using JaCore.Api.Repositories.Device;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Interfaces.Common;

namespace JaCore.Api.Configuration;

public static class RepositoriesConfig
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Register repositories
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IDeviceCardRepository, DeviceCardRepository>();

        return services;
    }
} 