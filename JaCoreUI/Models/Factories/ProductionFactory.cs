using JaCoreUI.Models.Productions.Base;
using JaCoreUI.Models.Productions.Preparation;
using JaCoreUI.Models.Productions.Template;
using JaCoreUI.Models.Productions.Work;
using System;

namespace JaCoreUI.Models.Factories;

/// <summary>
/// Factory for creating different types of productions
/// </summary>
public partial class ProductionFactory
{
    private readonly IProductionRepository _repository;

    public ProductionFactory(IProductionRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Creates a new template production
    /// </summary>
    public TemplateProduction CreateTemplate(string name, string createdBy)
    {
        return new TemplateProduction
        {
            Name = name,
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Creates a preparation production from a template
    /// </summary>
    public PreparationProduction CreatePreparationFromTemplate(Guid templateId)
    {
        var template = _repository.GetTemplateProduction(templateId);
        if (template == null)
            throw new ArgumentException("Template not found");

        return template.CreatePreparationProduction();
    }

    /// <summary>
    /// Creates a work production from a preparation
    /// </summary>
    public WorkProduction CreateWorkFromPreparation(Guid preparationId, string assignedTo)
    {
        var preparation = _repository.GetPreparationProduction(preparationId);
        if (preparation == null)
            throw new ArgumentException("Preparation not found");

        var work = preparation.CreateWorkProduction();
        work.AssignedTo = assignedTo;
        work.StartDate = DateTime.Now;
        work.Status = WorkStatus.InProgress;

        return work;
    }
}

/// <summary>
/// Interface for production data access
/// </summary>
public interface IProductionRepository
{
    TemplateProduction GetTemplateProduction(Guid id);
    PreparationProduction GetPreparationProduction(Guid id);
    WorkProduction GetWorkProduction(Guid id);
    void SaveProduction(Production production);
}