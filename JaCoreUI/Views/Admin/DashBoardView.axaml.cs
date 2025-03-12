using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using JaCoreUI.ViewModels.Admin;

namespace JaCoreUI.Views.Admin;

public partial class DashBoardView : UserControl
{
    public DashBoardView()
    {
        InitializeComponent();
        this.Loaded += DashBoardView_Loaded;
    }
    
    private void DashBoardView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Access ViewModel here
        if (DataContext is DashBoardViewModel viewModel)
        {
            Debug.WriteLine($"Productions count: {viewModel.Productions?.Count ?? 0}");
        }
    }
}