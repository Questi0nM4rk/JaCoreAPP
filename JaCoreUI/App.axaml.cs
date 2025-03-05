using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using JaCoreUI.Data;
using JaCoreUI.Factories;
using JaCoreUI.Services;
using JaCoreUI.ViewModels;
using JaCoreUI.ViewModels.Admin;
using JaCoreUI.ViewModels.Device;
using JaCoreUI.ViewModels.Production;
using JaCoreUI.ViewModels.Settings;
using JaCoreUI.ViewModels.Shell;
using JaCoreUI.ViewModels.Template;
using JaCoreUI.ViewModels.User;
using JaCoreUI.Views.Shell;
using Microsoft.Extensions.DependencyInjection;

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

// Register services
        collection
            .AddSingleton<ThemeService>();

// Register ViewModels
        collection
            .AddSingleton<ShellViewModel>()
            .AddTransient<DashboardViewModel>()
            .AddTransient<RegisterViewModel>()
            .AddTransient<DevicesViewModel>()
            .AddTransient<DeviceCreationViewModel>()
            .AddTransient<DeviceDetailsViewModel>()
            .AddTransient<ProductionsViewModel>()
            .AddTransient<ProductionWorkViewModel>()
            .AddTransient<ProductionCreationViewModel>()
            .AddTransient<SettingsViewModel>()
            .AddTransient<TemplatesViewModel>()
            .AddTransient<TemplateDetailsViewModel>()
            .AddTransient<LoginViewModel>();

// Register factory with correct generic parameters
        collection.AddSingleton<Func<ApplicationPageNames, PageViewModel>>(provider => name => name switch
        {
            ApplicationPageNames.Dashboard => provider.GetRequiredService<DashboardViewModel>(),
            ApplicationPageNames.Register => provider.GetRequiredService<RegisterViewModel>(),
            ApplicationPageNames.Devices => provider.GetRequiredService<DevicesViewModel>(),
            ApplicationPageNames.DeviceCreation => provider.GetRequiredService<DeviceCreationViewModel>(),
            ApplicationPageNames.DeviceDetails => provider.GetRequiredService<DeviceDetailsViewModel>(),
            ApplicationPageNames.Productions => provider.GetRequiredService<ProductionsViewModel>(),
            ApplicationPageNames.ProductionWork => provider.GetRequiredService<ProductionWorkViewModel>(),
            ApplicationPageNames.ProductionCreation => provider.GetRequiredService<ProductionCreationViewModel>(),
            ApplicationPageNames.Settings => provider.GetRequiredService<SettingsViewModel>(),
            ApplicationPageNames.Templates => provider.GetRequiredService<TemplatesViewModel>(),
            ApplicationPageNames.TemplateDetails => provider.GetRequiredService<TemplateDetailsViewModel>(),
            ApplicationPageNames.Login => provider.GetRequiredService<LoginViewModel>(),
            _ => throw new InvalidOperationException()
        });

        collection.AddSingleton<PageFactory>();

        // Build the service provider
        var services = collection.BuildServiceProvider();
            
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = new ShellView()
                {
                    DataContext  = services.GetRequiredService<ShellViewModel>()
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