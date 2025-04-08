using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Models.Core;
using JaCoreUI.Models.Elements;
using JaCoreUI.Models.Productions.Template;

namespace JaCoreUI.Services.Api;

public class ProductionApiService : ObservableObject
{
   private readonly ObservableCollection<Models.Productions.Base.Production> _productions;
   
   public ProductionApiService()
   {
       _productions = CreateProductions();
   }
   
   public ObservableCollection<Models.Productions.Base.Production> GetProductions() => _productions;

   private ObservableCollection<Models.Productions.Base.Production> CreateProductions()
   {
      return new ObservableCollection<Models.Productions.Base.Production>();
   }
   
   // =======

   public Models.Productions.Base.Production NewProduction()
   {
       return new TemplateProduction()
       {
           Id = _productions.Count,
           Name = "Nová Produkce",
           CreatedAt = DateTime.Now,
           CreatedBy = "Admin",
           Description = "This is a production",
           IsCompleted = false,
           ModifiedAt = DateTime.Now,
           ProductionTypeValue = ProductionType.Template,
           Steps = new ObservableCollection<Step>(),
           TemplateId = _productions.Count,
       };
   }
}