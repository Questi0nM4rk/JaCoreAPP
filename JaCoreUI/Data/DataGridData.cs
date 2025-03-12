using System.Collections.ObjectModel;

namespace JaCoreUI.Data
{
    public class DataGridDesignData
    {
        public ObservableCollection<SampleItem> SampleItems { get; } =
        [
            new SampleItem { Name = "Device 1", Status = "Online", LastUpdated = "2023-06-15" },
            new SampleItem { Name = "Device 2", Status = "Offline", LastUpdated = "2023-06-10" },
            new SampleItem { Name = "Device 3", Status = "Maintenance", LastUpdated = "2023-06-12" },
            new SampleItem { Name = "Device 1", Status = "Online", LastUpdated = "2023-06-15" },
            new SampleItem { Name = "Device 1", Status = "Online", LastUpdated = "2023-06-15" },
            new SampleItem { Name = "Device 1", Status = "Online", LastUpdated = "2023-06-15" },
            new SampleItem { Name = "Device 1", Status = "Online", LastUpdated = "2023-06-15" },
            new SampleItem { Name = "Device 1", Status = "Online", LastUpdated = "2023-06-15" },
        ];
    }

    public class SampleItem
    {
        public string? Name { get; set; }
        public string? Status { get; set; }
        public string? LastUpdated { get; set; }
    }
}