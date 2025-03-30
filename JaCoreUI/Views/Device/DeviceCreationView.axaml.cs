using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using JaCoreUI.ViewModels.Device;

namespace JaCoreUI.Views.Device;

public partial class DeviceCreationView : UserControl
{
    public DeviceCreationView()
    {
        InitializeComponent();
        this.Loaded += DeviceCreationLoaded;
    }

    private void DeviceCreationLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Access ViewModel here
        if (DataContext is DeviceCreationViewModel viewModel)
        {
            Debug.WriteLine($"Productions count: {viewModel.DeviceOperations?.Count ?? 0}");
        }
    }

}