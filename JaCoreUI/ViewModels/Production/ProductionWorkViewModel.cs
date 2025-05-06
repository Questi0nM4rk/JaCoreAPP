using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Data;
using System.Linq;
using JaCoreUI.ViewModels.Production;
using System.Collections.Generic;

namespace JaCoreUI.ViewModels.Production;

public partial class ProductionWorkViewModel()
    : PageViewModel(ApplicationPageNames.ProductionWork, ApplicationPageNames.Productions)
{
    [ObservableProperty]
    private ProductionViewModel _productionRootNode = new();

    [RelayCommand]
    private void AddNote(object item)
    {
        Console.WriteLine($"Add Note requested for item: {item?.GetType().Name}");
        // TODO: Implement logic to add a note (perhaps open a dialog or inline editor)
    }

    [RelayCommand]
    private void MarkIssue(object item)
    {
        Console.WriteLine($"Mark Issue requested for item: {item?.GetType().Name}");
        // TODO: Implement logic to mark an item as having an issue
    }

    protected override void OnDesignTimeConstructor()
    {
        ProductionRootNode = new ProductionViewModel { Name = "Execute: Production Order 123" };
        var step1 = new StepViewModel(ProductionRootNode) { Name = "Weighing" };
        var step2 = new StepViewModel(ProductionRootNode) { Name = "Mixing" };
        
        var instruction1_1 = new InstructionViewModel(step1) { Name = "Weigh Ingredient A", IsCompleted = null };
        instruction1_1.DynamicUiElements.Add(new DynamicUiElementViewModel(instruction1_1) { Model = new Models.Production.DynamicUiElementModel { Label = "Target Weight:", ElementType = Models.Production.DynamicUiElementType.TextReadOnly }, CalculatedValue = "12.0 kg"});
        instruction1_1.DynamicUiElements.Add(new DynamicUiElementViewModel(instruction1_1) { Model = new Models.Production.DynamicUiElementModel { Label = "Actual Weight", ElementType = Models.Production.DynamicUiElementType.NumericInput, Unit = "kg", BindingTargetProperty = nameof(InstructionViewModel.ActualValue1) }, IsWorkMode = true });

        var instruction1_2 = new InstructionViewModel(step1) { Name = "Record Batch ID" };
        instruction1_2.DynamicUiElements.Add(new DynamicUiElementViewModel(instruction1_2) { Model = new Models.Production.DynamicUiElementModel { Label = "Batch ID", ElementType = Models.Production.DynamicUiElementType.TextInput, BindingTargetProperty = nameof(InstructionViewModel.ActualText1) }, IsWorkMode = true });

        step1.Instructions.Add(instruction1_1);
        step1.Instructions.Add(instruction1_2);

        var subStep2_1 = new StepViewModel(step2) { Name = "Add Liquids" };
        var instruction2_1_1 = new InstructionViewModel(subStep2_1) { Name = "Add Water" };
        instruction2_1_1.IsCompleted = true;
        subStep2_1.Instructions.Add(instruction2_1_1);
        step2.SubSteps.Add(subStep2_1);
        step2.ChildCompletionChanged();

        ProductionRootNode.Steps.Add(step1);
        ProductionRootNode.Steps.Add(step2);
        ProductionRootNode.ChildStepCompletionChanged();
    }
}