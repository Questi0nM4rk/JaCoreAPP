using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Data.Common;

namespace JaCoreUI.Models.Elements.Device;

public partial class DeviceCard : ObservableObject
{
    [ObservableProperty]
    public required partial string SerialNumber { get; set; }
    
    [ObservableProperty]
    public required partial DateTime DateOfActivation { get; set; }
    
    [ObservableProperty]
    public partial ObservableCollection<Event>? Events { get; set; }
    
    [ObservableProperty]
    public partial MetrologicalConformation? MetrologicalConformation { get; set; }
    
    [ObservableProperty]
    public required partial Supplier Supplier { get; set; }
    
    [ObservableProperty]
    public partial Service? Service { get; set; }
    
}

public record MetrologicalConformation
{
    public string Level1 { get; set; } = string.Empty;
    public string Level2 { get; set; } = string.Empty;
    public string Level3 { get; set; } = string.Empty;
    public string Level4 { get; set; } = string.Empty;
}