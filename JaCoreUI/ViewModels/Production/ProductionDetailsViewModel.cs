using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Data;
using System.Collections.ObjectModel;
using System.Linq;
using JaCoreUI.ViewModels.Production;
using System.Collections.Generic;

namespace JaCoreUI.ViewModels.Production;

public partial class ProductionDetailsViewModel()
    : PageViewModel(ApplicationPageNames.ProductionDetails, ApplicationPageNames.Productions)
{
    // In a real app, these would be loaded via a service
    [ObservableProperty]
    private ObservableCollection<ProductionViewModel> _availableTemplates = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProductionRootNode))]
    private ProductionViewModel? _selectedTemplate;

    [ObservableProperty]
    private double? _targetProductionAmount = 100;

    [ObservableProperty]
    private string? _selectedTargetUnit = "kg";

    [ObservableProperty]
    private ObservableCollection<string> _availableUnits = ["kg", "L", "pcs"];

    /// <summary>
    /// The production instance being prepared (potentially cloned from SelectedTemplate).
    /// Displayed in a read-only fashion in this view.
    /// </summary>
    [ObservableProperty]
    private ProductionViewModel? _productionRootNode;

    partial void OnSelectedTemplateChanged(ProductionViewModel? value)
    {
        if (value != null)
        {
            ProductionRootNode = value; 
        }
        else
        {
            ProductionRootNode = null;
        }
    }

    partial void OnTargetProductionAmountChanged(double? value)
    {
        // TODO: Recalculate dynamic element values in ProductionRootNode when target amount changes
    }
     partial void OnSelectedTargetUnitChanged(string? value)
    {
         // TODO: Recalculate dynamic element values if unit conversion is involved
    }


    [RelayCommand]
    private void PublishToWork()
    {
        Console.WriteLine("Publish to Work command executed.");
    }

    protected override void OnDesignTimeConstructor()
    {
        AvailableUnits = ["kg", "L", "pcs"];
        SelectedTargetUnit = "kg";
    }
}