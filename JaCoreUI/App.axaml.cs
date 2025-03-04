using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Services;
using JaCoreUI.ViewModels;
using JaCoreUI.ViewModels.Admin;
using JaCoreUI.ViewModels.Settings;
using JaCoreUI.ViewModels.Shell;
using JaCoreUI.Views.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace JaCoreUI;

public partial class App : Application
{
    public IServiceProvider? Services { get; private set; }
    public new static App? Current => Application.Current as App;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Services = ConfigureServices();
    }

    private static ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddTransient<Program>()
            .AddSingleton<ThemeService>()
            .AddTransient<ShellViewModel>()
            .AddTransient<DashboardViewModel>()
            .AddTransient<SettingsViewModel>()
            .BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = Services?.GetRequiredService<ShellViewModel>();
            desktop.MainWindow = new ShellView { DataContext = vm };
        }
        base.OnFrameworkInitializationCompleted();
    }
}