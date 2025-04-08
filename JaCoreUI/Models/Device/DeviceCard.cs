using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JaCoreUI.Models.Device;

public partial class DeviceCard : ObservableObject
{
    [ObservableProperty] public required partial string SerialNumber { get; set; }

    [ObservableProperty] public required partial DateTimeOffset? DateOfActivation { get; set; }

    [ObservableProperty] public partial ObservableCollection<Event>? Events { get; set; }

    [ObservableProperty] public partial MetrologicalConformation? MetrologicalConformation { get; set; }

    [ObservableProperty] public partial Supplier? Supplier { get; set; }

    [ObservableProperty] public partial Service? Service { get; set; }

    public bool HasEvents => Events is { Count: > 0 };

    public bool HasMetrologicalConformation => MetrologicalConformation is not null;

    public bool HasService => Service is not null;

    public bool HasSupplier => Supplier is not null;
}

public record MetrologicalConformation
{
    public string Level1 { get; set; } = string.Empty;
    public string Level2 { get; set; } = string.Empty;
    public string Level3 { get; set; } = string.Empty;
    public string Level4 { get; set; } = string.Empty;
}