using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Data;

namespace JaCoreUI.ViewModels.Admin
{
    public partial class DashBoardViewModel : PageViewModel
    {
        [ObservableProperty]
        public partial ObservableCollection<DummyWorkProduction> Productions { get; set; } = [];

        [ObservableProperty]
        public partial ObservableCollection<DummyDevice> Devices { get; set; } = [];
        
        [ObservableProperty]
        public partial string ProductionSearchText { get; set; } = string.Empty;
        
        [ObservableProperty]
        public partial string DeviceSearchText { get; set; } = string.Empty;

        public DashBoardViewModel() : base(ApplicationPageNames.Dashboard)
        {
            InitializeDummyData();
        }

        private void InitializeDummyData()
        {
            // Initialize Productions collection
            Productions =
            [
                new()
                {
                    Id = Guid.NewGuid(), Name = "Production A-123", Status = DummyWorkStatus.InProgress,
                    StartDate = DateTime.Now.AddDays(-5), AssignedTo = "John Smith"
                },

                new()
                {
                    Id = Guid.NewGuid(), Name = "Production B-456", Status = DummyWorkStatus.Completed,
                    StartDate = DateTime.Now.AddDays(-10), CompletionDate = DateTime.Now.AddDays(-1),
                    AssignedTo = "Emma Johnson"
                },

                new()
                {
                    Id = Guid.NewGuid(), Name = "Production C-789", Status = DummyWorkStatus.OnHold,
                    StartDate = DateTime.Now.AddDays(-3), AssignedTo = "Michael Brown"
                },

                new()
                {
                    Id = Guid.NewGuid(), Name = "Production D-012", Status = DummyWorkStatus.NotStarted,
                    AssignedTo = "Lisa Chen"
                }
            ];

            // Initialize Devices collection
            Devices =
            [
                new()
                {
                    Id = Guid.NewGuid(), Name = "Conveyor Belt A1", DeviceType = "Conveyor",
                    SerialNumber = "CBT-2025-001", Status = DummyDeviceStatus.Online,
                    LastMaintenance = DateTime.Now.AddMonths(-2)
                },

                new()
                {
                    Id = Guid.NewGuid(), Name = "Robotic Arm B3", DeviceType = "Robot",
                    SerialNumber = "ROB-2024-153", Status = DummyDeviceStatus.Maintenance,
                    LastMaintenance = DateTime.Now.AddDays(-1)
                },

                new()
                {
                    Id = Guid.NewGuid(), Name = "Sensor Array C5", DeviceType = "Sensor",
                    SerialNumber = "SNS-2025-089", Status = DummyDeviceStatus.Offline,
                    LastMaintenance = DateTime.Now.AddMonths(-5)
                },

                new()
                {
                    Id = Guid.NewGuid(), Name = "CNC Machine D2", DeviceType = "Machining",
                    SerialNumber = "CNC-2023-421", Status = DummyDeviceStatus.Online,
                    LastMaintenance = DateTime.Now.AddMonths(-1)
                },

                new()
                {
                    Id = Guid.NewGuid(), Name = "Packaging Unit E7", DeviceType = "Packaging",
                    SerialNumber = "PKG-2024-076", Status = DummyDeviceStatus.Online,
                    LastMaintenance = DateTime.Now.AddDays(-15)
                }
            ];
        }
        
        public Func<string, CancellationToken, Task<IEnumerable<DummyWorkProduction>>> SearchProductions => 
            async (searchText, cancellationToken) =>
            {
                // Simulate a delay like an API call would have
                await Task.Delay(100, cancellationToken);
                
                if (string.IsNullOrWhiteSpace(searchText))
                    return Productions;
                    
                return Productions.Where(p => 
                    p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) || 
                    p.AssignedTo.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            };
            
        public Func<string, CancellationToken, Task<IEnumerable<DummyDevice>>> SearchDevices => 
            async (searchText, cancellationToken) =>
            {
                await Task.Delay(100, cancellationToken);
                
                if (string.IsNullOrWhiteSpace(searchText))
                    return Devices;
                    
                return Devices.Where(d => 
                    d.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) || 
                    d.DeviceType.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    d.SerialNumber.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            };

        [RelayCommand]
        private void EditProduction(Guid id)
        {
            // Editing logic would go here
            var production = Productions.FirstOrDefault(p => p.Id == id);
            if (production != null)
            {
                // Open edit dialog or navigate to edit page
            }
        }
        
        [RelayCommand]
        private void ViewProductionDetails(Guid id)
        {
            // View details logic would go here
            var production = Productions.FirstOrDefault(p => p.Id == id);
            if (production != null)
            {
                // Navigate to details page or show details dialog
            }
        }
        
        [RelayCommand]
        private void ViewDeviceDetails(Guid id)
        {
            // View device details logic would go here
            var device = Devices.FirstOrDefault(d => d.Id == id);
            if (device != null)
            {
                // Navigate to device details page or show details dialog
            }
        }

        protected override void OnDesignTimeConstructor()
        {
            throw new NotImplementedException();
        }
        
    }
    
    // Existing dummy classes
    public class DummyWorkProduction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = "Sample production process";
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? CompletionDate { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public DummyWorkStatus Status { get; set; }
        public bool IsCompleted => Status == DummyWorkStatus.Completed;
    }
    
    public enum DummyWorkStatus
    {
        NotStarted,
        InProgress,
        OnHold,
        Completed,
        Cancelled
    }
    
    public class DummyDevice
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Description { get; set; } = "Sample device for production line";
        public DummyDeviceStatus Status { get; set; } = DummyDeviceStatus.Online;
        public DateTime LastMaintenance { get; set; } = DateTime.Now.AddMonths(-1);
        public DateTime NextScheduledMaintenance => LastMaintenance.AddMonths(3);
        public bool NeedsMaintenance => (NextScheduledMaintenance - DateTime.Now).TotalDays < 14;
                
        public override string ToString()
        {
            return $"{Name} ({DeviceType}) - {Status}";
        }
    }
    
    public enum DummyDeviceStatus
    {
        Online,
        Offline,
        Maintenance,
        Error
    }
}
