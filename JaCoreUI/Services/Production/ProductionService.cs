using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JaCoreUI.Data;
using JaCoreUI.Models.Device;
using JaCoreUI.Models.UI;
using JaCoreUI.Services.Api;
using MsBox.Avalonia.Enums;

namespace JaCoreUI.Services.Production;

public partial class ProductionService : ObservableObject
{
    private readonly Navigation.CurrentPageService _currentPageService;
    private readonly ProductionApiService _productionApiService;
    
    public Models.Productions.Base.Production? CurrentProduction { get; set; }

    public Models.Productions.Base.Production? TempProduction { get; set; }
    
    [ObservableProperty] public partial ObservableCollection<Models.Productions.Base.Production> Productions { get; set; }
    

    public ProductionService(ProductionApiService productionApiService, Navigation.CurrentPageService currentPageService)
    {
        Productions = new ObservableCollection<Models.Productions.Base.Production>();
        
        _currentPageService = currentPageService;
        _productionApiService = productionApiService;
        
        LoadProductionsAsync();
    }

    private async void LoadProductionsAsync()
    {
        try
        {
            var productions = await _productionApiService.GetProductionsAsync();
            
            foreach (var production in productions)
            {
                Productions.Add(production);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading productions: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SaveProduction()
    {
        await ErrorDialog.ShowWithButtonsAsync("Save production", "Save production", ButtonEnum.Ok);
    }
    
    [RelayCommand]
    private async Task ProductionDetails(int id)
    {
        var production = Productions.FirstOrDefault(p => p.Id == id);

        CurrentProduction = production ?? throw new ArgumentNullException(nameof(production));
        await _currentPageService.NavigateTo(ApplicationPageNames.ProductionDetails);
    }
        
    [RelayCommand]
    private async Task NewDevice(int id)
    {
        var newProduction = new Models.Productions.Template.TemplateProduction
        {
            Name = "New Production",
            Description = "Description",
            CreatedAt = DateTime.Now,
            ModifiedAt = DateTime.Now
        };
        
        try 
        {
            CurrentProduction = await _productionApiService.CreateProductionAsync(newProduction);
            Productions.Add(CurrentProduction);
            await _currentPageService.NavigateTo(ApplicationPageNames.DeviceDetails);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating production: {ex.Message}");
        }
    }
}