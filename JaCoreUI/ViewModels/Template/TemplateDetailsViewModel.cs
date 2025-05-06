using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Data;
using System.Collections.ObjectModel;
using System.Linq;
using JaCoreUI.ViewModels.Production;

namespace JaCoreUI.ViewModels.Template;

public partial class TemplateDetailsViewModel()
    : PageViewModel(ApplicationPageNames.TemplateDetails, ApplicationPageNames.Templates)
{
    [ObservableProperty]
    private ProductionViewModel _productionRootNode = new();

    [ObservableProperty]
    private double? _templateBaseAmount = 100;

    [ObservableProperty]
    private string? _selectedBaseUnit = "kg";

    [ObservableProperty]
    private ObservableCollection<string> _availableUnits = ["kg", "L", "pcs"];

    [RelayCommand]
    private void AddRootStep()
    {
        ProductionRootNode.Steps.Add(new StepViewModel(ProductionRootNode) { Name = "New Root Step" });
    }

    [RelayCommand]
    private void SaveTemplate()
    {
        Console.WriteLine("Save Template command executed.");
    }

    [RelayCommand]
    private void AddSubStep(StepViewModel parentStep)
    {
        parentStep?.SubSteps.Add(new StepViewModel(parentStep) { Name = "New Sub Step" });
    }

    [RelayCommand]
    private void AddInstruction(StepViewModel parentStep)
    {
        parentStep?.Instructions.Add(new InstructionViewModel(parentStep) { Name = "New Instruction" });
    }

    [RelayCommand]
    private void RemoveItem(ViewModelBase item)
    {
        if (item is StepViewModel step)
        {
             if (ProductionRootNode.Steps.Contains(step))
                 ProductionRootNode.Steps.Remove(step);
             else 
             {
                 Console.WriteLine($"Remove Step requested (potentially nested): {step.Name}");
             }
        }
        else if (item is InstructionViewModel instruction)
        {
             Console.WriteLine($"Remove Instruction requested: {instruction.Name}");
        }
    }

    [RelayCommand]
    private void ConfigureUiElements(InstructionViewModel instruction)
    {
        Console.WriteLine($"Configure UI Elements requested for: {instruction?.Name}");
        if (instruction != null && !instruction.DynamicUiElements.Any())
        {
            instruction.DynamicUiElements.Add(new DynamicUiElementViewModel(instruction)
            {
                Model = new Models.Production.DynamicUiElementModel 
                { 
                    ElementType = Models.Production.DynamicUiElementType.TextInput,
                    Label = "Example Input",
                    BindingTargetProperty = "ActualText1"
                }
            });
        }
    }

    protected override void OnDesignTimeConstructor()
    {
        ProductionRootNode = new ProductionViewModel { Name = "Design Time Template" };
        var step1 = new StepViewModel(ProductionRootNode) { Name = "Weighing" };
        var step2 = new StepViewModel(ProductionRootNode) { Name = "Mixing" };
        var instruction1_1 = new InstructionViewModel(step1) { Name = "Weigh Ingredient A" };
        instruction1_1.DynamicUiElements.Add(new DynamicUiElementViewModel(instruction1_1) { Model = new Models.Production.DynamicUiElementModel { Label = "Target Weight", ElementType = Models.Production.DynamicUiElementType.Label } });
        instruction1_1.DynamicUiElements.Add(new DynamicUiElementViewModel(instruction1_1) { Model = new Models.Production.DynamicUiElementModel { Label = "Actual Weight", ElementType = Models.Production.DynamicUiElementType.NumericInput, Unit = "kg", BindingTargetProperty = "ActualValue1" } });

        var instruction1_2 = new InstructionViewModel(step1) { Name = "Record Batch ID" };
         instruction1_2.DynamicUiElements.Add(new DynamicUiElementViewModel(instruction1_2) { Model = new Models.Production.DynamicUiElementModel { Label = "Batch ID", ElementType = Models.Production.DynamicUiElementType.TextInput, BindingTargetProperty = "ActualText1" } });

        step1.Instructions.Add(instruction1_1);
        step1.Instructions.Add(instruction1_2);

        var subStep2_1 = new StepViewModel(step2) { Name = "Add Liquids" };
        var instruction2_1_1 = new InstructionViewModel(subStep2_1) { Name = "Add Water" };
        step2.SubSteps.Add(subStep2_1);
        subStep2_1.Instructions.Add(instruction2_1_1);

        ProductionRootNode.Steps.Add(step1);
        ProductionRootNode.Steps.Add(step2);

        TemplateBaseAmount = 100;
        SelectedBaseUnit = "kg";
        AvailableUnits = ["kg", "L", "pcs"];
    }
}