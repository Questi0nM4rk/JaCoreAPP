using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace JaCoreUI.ViewModels.Production;

/// <summary>
/// Represents the root production process or template.
/// </summary>
public partial class ProductionViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _name = "New Production";

    [ObservableProperty]
    private double? _baseAmount;

    [ObservableProperty]
    private string? _baseUnit;

    [ObservableProperty]
    private double? _targetAmount;

    [ObservableProperty]
    private string? _targetUnit;

    // Hierarchical Items
    [ObservableProperty]
    private ObservableCollection<StepViewModel> _steps = [];

    // Completion Status (Calculated based on children)
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressStatus))]
    private bool? _isCompleted = false; // Default to not started

    [ObservableProperty]
    private bool _canComplete = false; // Calculated based on children

    public string ProgressStatus => IsCompleted switch
    {
        true => "Completed",
        false => "NotStarted",
        null => "InProgress"
    };

    // TODO: Add logic to recalculate IsCompleted/CanComplete when child Steps change.
    // TODO: Add logic for scaling values from BaseAmount to TargetAmount.

    public ProductionViewModel()
    {
        // Example initialization
        Steps.CollectionChanged += (s, e) => RecalculateCompletion();
    }

    private void RecalculateCompletion()
    {
        if (!Steps.Any())
        {
            IsCompleted = false;
            CanComplete = true; // An empty production is technically completable
            return;
        }

        bool allChildrenComplete = Steps.All(s => s.IsCompleted == true);
        bool anyChildIncomplete = Steps.Any(s => s.IsCompleted == false || s.IsCompleted == null);
        bool allChildrenCompletable = Steps.All(s => s.CanComplete);

        if (allChildrenComplete)
        {
            IsCompleted = true;
        }
        else if (anyChildIncomplete)
        {
            IsCompleted = null; // In Progress
        }
        else // All children are NotStarted
        {
            IsCompleted = false;
        }

        // Can complete production if all steps *can* be completed (even if not yet started)
        CanComplete = allChildrenCompletable;
    }

    // Method to be called when a child step's completion state changes
    public void ChildStepCompletionChanged()
    {
        RecalculateCompletion();
        // Potentially notify parent if this ProductionViewModel is nested
    }
} 