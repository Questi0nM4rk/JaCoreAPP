using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Data;
using JaCoreUI.Services;
using DeviceService = JaCoreUI.Services.Device.DeviceService;
using ProductionService = JaCoreUI.Services.Production.ProductionService;

namespace JaCoreUI.ViewModels.Admin;

public partial class DashBoardViewModel(ProductionService productionService, DeviceService deviceService)
    : PageViewModel(ApplicationPageNames.Dashboard, ApplicationPageNames.Dashboard)
{
    public ProductionService ProductionService { get; set; } = productionService;
    public DeviceService DeviceService { get; set; } = deviceService;

    [ObservableProperty] public partial string ProductionSearchText { get; set; } = string.Empty;

    [ObservableProperty] public partial string DeviceSearchText { get; set; } = string.Empty;

    public IRelayCommand ProductionDetailsCommand => ProductionService.ProductionDetailsCommand;
    public IRelayCommand DeviceDetailsCommand => DeviceService.DeviceDetailsCommand;
    
    protected override void OnDesignTimeConstructor()
    {
        throw new NotImplementedException();
    }

    public override bool Validate()
    {
        return true;
    }
}