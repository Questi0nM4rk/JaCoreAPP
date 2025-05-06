using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace JaCoreUI.ViewModels.Production;

/// <summary>
/// Represents a step within a production process.
/// </summary>
public partial class StepViewModel : ViewModelBase
{
    private readonly ProductionViewModel? _parentProduction; // To notify parent
    private readonly StepViewModel? _parentStep; // To notify parent

    [ObservableProperty]
    private string _name = "New Step";

    // Hierarchical Items
    [ObservableProperty]
    private ObservableCollection<InstructionViewModel> _instructions = [];

    [ObservableProperty]
    private ObservableCollection<StepViewModel> _subSteps = []; // Allow nested steps

    // Completion Status (Calculated based on children: Instructions and SubSteps)
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressStatus))]
    [NotifyPropertyChangedFor(nameof(CanComplete))] // CanComplete depends on IsCompleted state of children
    private bool? _isCompleted = false; // Default to not started

    public bool CanComplete
    {
        get
        {
            // Can complete if all direct instructions are completed or completable,
            // AND all sub-steps are completed or completable.
            bool allInstructionsOk = Instructions.All(i => i.IsCompleted == true);
            bool allSubStepsOk = SubSteps.All(s => s.IsCompleted == true);
            return allInstructionsOk && allSubStepsOk;
        }
    }

    public string ProgressStatus => IsCompleted switch
    {
        true => "Completed",
        false => "NotStarted",
        null => "InProgress"
    };

    // Constructor accepting parent for notification
    public StepViewModel(ProductionViewModel parent)
    {
        _parentProduction = parent;
        InitializeCollections();
    }

    // Constructor for nested steps
    public StepViewModel(StepViewModel parent)
    {
        _parentStep = parent;
        InitializeCollections();
    }

    // Design-time constructor
    public StepViewModel()
    {
        InitializeCollections();
    }

    private void InitializeCollections()
    {
        Instructions.CollectionChanged += (s, e) => RecalculateCompletion();
        SubSteps.CollectionChanged += (s, e) => RecalculateCompletion();
    }

    partial void OnIsCompletedChanged(bool? value)
    {
        // When explicitly set (e.g., by user checking box in Work mode),
        // potentially mark all children as complete? Or is this only ever calculated?
        // For now, assume it's primarily calculated, but notify parent.
        NotifyParentCompletionChanged();
    }

    private void RecalculateCompletion()
    {
        var childItems = Instructions.Cast<object>().Concat(SubSteps.Cast<object>()).ToList();

        if (!childItems.Any())
        {
            IsCompleted = false; // An empty step starts as not started
            // CanComplete should remain true for empty step
            OnPropertyChanged(nameof(CanComplete));
            NotifyParentCompletionChanged();
            return;
        }

        bool allChildrenComplete = Instructions.All(i => i.IsCompleted == true) && SubSteps.All(s => s.IsCompleted == true);
        bool anyChildInProgress = Instructions.Any(i => i.IsCompleted == null) || SubSteps.Any(s => s.IsCompleted == null);
        bool anyChildNotStarted = Instructions.Any(i => i.IsCompleted == false) || SubSteps.Any(s => s.IsCompleted == false);

        bool? oldCompleted = _isCompleted;

        if (allChildrenComplete)
        {
            IsCompleted = true;
        }
        else if (anyChildInProgress || (anyChildNotStarted && !allChildrenComplete && !Instructions.All(i => i.IsCompleted == false) && !SubSteps.All(s => s.IsCompleted == false)))
        {
            // If any child is InProgress OR (some are NotStarted and some are Complete)
            IsCompleted = null; // In Progress
        }
        else // All children must be NotStarted
        {
            IsCompleted = false;
        }

        // Update CanComplete property explicitly as it depends on child states
        OnPropertyChanged(nameof(CanComplete));

        if (oldCompleted != _isCompleted)
        {
             NotifyParentCompletionChanged();
        }
    }

    // Method called by child Instruction or SubStep
    public void ChildCompletionChanged()
    {
        RecalculateCompletion();
    }

    private void NotifyParentCompletionChanged()
    {
        _parentProduction?.ChildStepCompletionChanged();
        _parentStep?.ChildCompletionChanged();
    }
} 