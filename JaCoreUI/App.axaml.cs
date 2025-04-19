using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using JaCoreUI.Data;
using JaCoreUI.Factories;
using JaCoreUI.Services;
using JaCoreUI.Services.Api;
using JaCoreUI.Services.User;
using JaCoreUI.ViewModels;
using JaCoreUI.ViewModels.Admin;
using JaCoreUI.ViewModels.Device;
using JaCoreUI.ViewModels.Production;
using JaCoreUI.ViewModels.Settings;
using JaCoreUI.ViewModels.Shell;
using JaCoreUI.ViewModels.Template;
using JaCoreUI.ViewModels.User;
using JaCoreUI.Views.Shell;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CurrentPageService = JaCoreUI.Services.Navigation.CurrentPageService;
using DeviceService = JaCoreUI.Services.Device.DeviceService;
using ProductionService = JaCoreUI.Services.Production.ProductionService;
using ThemeService = JaCoreUI.Services.Theme.ThemeService;
using AuthService = JaCoreUI.Services.User.AuthService;

[assembly: XmlnsDefinition("https://github.com/avaloniaui", "JaCoreUI.Controls")]

namespace JaCoreUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Register configuration
        collection.AddSingleton<IConfiguration>(configuration);

        // Register API services first (dependencies)
        collection
            .AddSingleton<DeviceApiService>()
            .AddSingleton<ProductionApiService>()
            .AddSingleton<UserApiService>();

        // Register core services
        collection
            .AddSingleton<AuthService>()
            .AddSingleton<ThemeService>();

        // Register PageFactory before services that depend on it
        collection.AddSingleton<PageFactory>();
            
        // Register application services
        collection
            .AddSingleton<DeviceService>()
            .AddSingleton<UserService>()
            .AddSingleton<CurrentPageService>()
            .AddSingleton<ProductionService>();

        // Register ViewModels
        collection
            .AddSingleton<ShellViewModel>();
            
        collection    
            .AddTransient<DashBoardViewModel>()
            .AddTransient<RegisterViewModel>()
            .AddTransient<DevicesViewModel>()
            .AddTransient<DeviceDetailsViewModel>()
            .AddTransient<ProductionsViewModel>()
            .AddTransient<ProductionWorkViewModel>()
            .AddTransient<ProductionDetailsViewModel>()
            .AddTransient<SettingsViewModel>()
            .AddTransient<TemplatesViewModel>()
            .AddTransient<TemplateDetailsViewModel>()
            .AddTransient<LoginViewModel>();

        // Register factory with correct generic parameters
        collection.AddSingleton<Func<ApplicationPageNames, PageViewModel>>(provider => name
            => name switch
            {
                ApplicationPageNames.Dashboard => provider.GetRequiredService<DashBoardViewModel>(),
                ApplicationPageNames.Register => provider.GetRequiredService<RegisterViewModel>(),
                ApplicationPageNames.Devices => provider.GetRequiredService<DevicesViewModel>(),
                ApplicationPageNames.DeviceDetails => provider.GetRequiredService<DeviceDetailsViewModel>(),
                ApplicationPageNames.Productions => provider.GetRequiredService<ProductionsViewModel>(),
                ApplicationPageNames.ProductionWork => provider.GetRequiredService<ProductionWorkViewModel>(),
                ApplicationPageNames.ProductionDetails => provider.GetRequiredService<ProductionDetailsViewModel>(),
                ApplicationPageNames.Settings => provider.GetRequiredService<SettingsViewModel>(),
                ApplicationPageNames.Templates => provider.GetRequiredService<TemplatesViewModel>(),
                ApplicationPageNames.TemplateDetails => provider.GetRequiredService<TemplateDetailsViewModel>(),
                ApplicationPageNames.Login => provider.GetRequiredService<LoginViewModel>(),
                _ => throw new InvalidOperationException()
            });

        // Build the service provider
        var services = collection.BuildServiceProvider();
        
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = new ShellView()
                {
                    DataContext = services.GetRequiredService<ShellViewModel>()
                };
                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = new ShellView()
                {
                    DataContext = services.GetRequiredService<ShellViewModel>()
                };
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }
}