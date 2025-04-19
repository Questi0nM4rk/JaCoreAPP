using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using JaCore.Common.Device;

namespace JaCoreUI.Models.Device;

public partial class DeviceCard : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty] public partial string? SerialNumber { get; set; }
    [ObservableProperty] public partial DateTimeOffset? DateOfActivation { get; set; }
    [ObservableProperty] public partial ObservableCollection<int>? EventIds { get; set; }
    [ObservableProperty] public partial MetrologicalConformation? MetrologicalConformation { get; set; }
    [ObservableProperty] public partial Supplier? Supplier { get; set; }
    [ObservableProperty] public partial int? SupplierId { get; set; }
    [ObservableProperty] public partial Service? Service { get; set; }
    [ObservableProperty] public partial int? ServiceId { get; set; }

    public bool HasEvents => EventIds is { Count: > 0 };
    public bool HasMetrologicalConformation => MetrologicalConformation is not null;
    public bool HasService => Service is not null;
    public bool HasSupplier => Supplier is not null;
    
    public DeviceCard() { }
    
    public DeviceCard(DeviceCard source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Id = source.Id;
        SerialNumber = source.SerialNumber;
        DateOfActivation = source.DateOfActivation;
        
        if (source.MetrologicalConformation != null)
            MetrologicalConformation = source.MetrologicalConformation with { };
        
        if (source.Supplier != null)
            Supplier = new Supplier(source.Supplier);
        
        if (source.Service != null)
            Service = new Service(source.Service);

        EventIds = source.EventIds != null ? new ObservableCollection<int>(source.EventIds) : new ObservableCollection<int>();
    }
    
    public DeviceCard Clone() => new DeviceCard(this);
}
