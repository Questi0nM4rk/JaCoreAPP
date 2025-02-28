using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Services;
using JaCoreUI.ViewModels;
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
            .AddTransient<MainWindowViewModel>()
            .AddTransient<DashboardViewModel>()
            .AddTransient<SettingsViewModel>()
            .BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = Services?.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow { DataContext = vm };
        }
        base.OnFrameworkInitializationCompleted();
    }
}