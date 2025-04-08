using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Data;
using JaCoreUI.Models.Device;
using JaCoreUI.Services.Api;

namespace JaCoreUI.Services.Production;

public partial class ProductionService : ObservableObject
{
    private readonly Navigation.CurrentPageService _currentPageService;
    private readonly ProductionApiService _productionApiService;
    
    public Models.Productions.Base.Production? CurrentProduction { get; set; }

    [ObservableProperty] public partial ObservableCollection<Models.Productions.Base.Production> Productions { get; set; }
    

    public ProductionService(ProductionApiService productionApiService, Navigation.CurrentPageService currentPageService)
    {
        Productions = productionApiService.GetProductions();
        
        _currentPageService = currentPageService;
        _productionApiService = productionApiService;
    }
    
    
    [RelayCommand]
    private void ProductionDetails(int id)
    {
        var production = Productions.FirstOrDefault(p => p.Id == id);

        CurrentProduction = production ?? throw new ArgumentNullException(nameof(production));
        _currentPageService.NavigateTo(ApplicationPageNames.ProductionDetails);
    }
        
    [RelayCommand]
    private void NewDevice(int id)
    {
        CurrentProduction = _productionApiService.NewProduction();
        Productions.Add(CurrentProduction);
        _currentPageService.NavigateTo(ApplicationPageNames.DeviceDetails);
    }
}