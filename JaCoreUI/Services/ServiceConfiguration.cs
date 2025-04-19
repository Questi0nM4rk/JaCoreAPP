using JaCoreUI.Services.Api;
using JaCoreUI.Services.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JaCoreUI.Services;

/// <summary>
/// Configuration for application services
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Registers all services required by the application
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The application configuration</param>
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register API services
        services.AddSingleton<DeviceApiService>();
        services.AddSingleton<ProductionApiService>();
        
        // Register authentication services
        services.AddSingleton<AuthService>();
        
        // Register device services
        services.AddSingleton<Device.DeviceService>();
        
        // Register user services
        services.AddSingleton<User.UserService>();
        
        // Register navigation services
        services.AddSingleton<Navigation.CurrentPageService>();
        
        // Register theme services
        services.AddSingleton<Theme.ThemeService>();
    }
} 