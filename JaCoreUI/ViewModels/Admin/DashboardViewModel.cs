using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Data;

namespace JaCoreUI.ViewModels.Admin;

public partial class DashboardViewModel : PageViewModel
{    
    protected override void OnDesignTimeConstructor()
    {
        throw new NotImplementedException();
    }
    
    [ObservableProperty]
    private partial ObservableCollection<DummyWorkProduction> Productions { get; set; } = new();
    
    public DashboardViewModel() : base(ApplicationPageNames.Dashboard)
    {
        InitializeDummyData();
    }
    
    private void InitializeDummyData()
    {
        Productions = new ObservableCollection<DummyWorkProduction>
        {
            new() { Id = Guid.NewGuid(), Name = "Production A-123", Status = DummyWorkStatus.InProgress, 
                   StartDate = DateTime.Now.AddDays(-5), AssignedTo = "John Smith" },
            new() { Id = Guid.NewGuid(), Name = "Production B-456", Status = DummyWorkStatus.Completed, 
                   StartDate = DateTime.Now.AddDays(-10), CompletionDate = DateTime.Now.AddDays(-1), 
                   AssignedTo = "Emma Johnson" },
            new() { Id = Guid.NewGuid(), Name = "Production C-789", Status = DummyWorkStatus.OnHold, 
                   StartDate = DateTime.Now.AddDays(-3), AssignedTo = "Michael Brown" },
            new() { Id = Guid.NewGuid(), Name = "Production D-012", Status = DummyWorkStatus.NotStarted, 
                   AssignedTo = "Lisa Chen" }
        };
    }
    
    // Standalone dummy classes - contained entirely within this file
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
        
        // Add any other properties needed for your UI that match your real WorkProduction
    }
    
    public enum DummyWorkStatus
    {
        NotStarted,
        InProgress,
        OnHold,
        Completed,
        Cancelled
    }


}
