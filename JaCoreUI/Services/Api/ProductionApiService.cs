using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Models.Core;
using JaCoreUI.Models.Elements;
using JaCoreUI.Models.Productions.Template;
using Microsoft.Extensions.Configuration;

namespace JaCoreUI.Services.Api;

/// <summary>
/// Service for interacting with the Production API endpoints
/// </summary>
public class ProductionApiService : ApiClientBase
{
    public ProductionApiService(IConfiguration configuration) : base(configuration)
    {
    }
    
    /// <summary>
    /// Gets productions with pagination
    /// </summary>
    public async Task<ObservableCollection<Models.Productions.Base.Production>> GetProductionsAsync(int page = 0, int pageSize = 20)
    {
        try
        {
            var dtos = await GetAsync<List<ProductionDto>>($"Production?pageNumber={page}&pageSize={pageSize}");
            
            var productions = new ObservableCollection<Models.Productions.Base.Production>();
            
            foreach (var dto in dtos)
            {
                productions.Add(MapFromDto(dto));
            }
            
            return productions;
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Error retrieving productions: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Gets a production by ID
    /// </summary>
    public async Task<Models.Productions.Base.Production> GetProductionByIdAsync(int id)
    {
        try
        {
            var dto = await GetAsync<ProductionDto>($"Production/{id}");
            return MapFromDto(dto);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Error retrieving production: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Creates a new production
    /// </summary>
    public async Task<Models.Productions.Base.Production> CreateProductionAsync(Models.Productions.Base.Production production)
    {
        try
        {
            var createDto = new CreateProductionDto
            {
                Name = production.Name ?? string.Empty,
                Description = production.Description
            };
            
            var dto = await PostAsync<CreateProductionDto, ProductionDto>("Production", createDto);
            return MapFromDto(dto);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Error creating production: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Updates an existing production
    /// </summary>
    public async Task UpdateProductionAsync(Models.Productions.Base.Production production)
    {
        try
        {
            var updateDto = new UpdateProductionDto
            {
                Name = production.Name ?? string.Empty,
                Description = production.Description
            };
            
            await PutAsync($"Production/{production.Id}", updateDto);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Error updating production: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Deletes a production
    /// </summary>
    public async Task DeleteProductionAsync(int id)
    {
        try
        {
            await DeleteAsync($"Production/{id}");
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException(ApiErrorType.Unknown, $"Error deleting production: {ex.Message}", ex);
        }
    }
    
    #region Helpers
    
    private Models.Productions.Base.Production MapFromDto(ProductionDto dto)
    {
        var production = new Models.Productions.Template.TemplateProduction
        {
            Id = dto.Id,
            Name = dto.Name ?? string.Empty,
            Description = dto.Description,
            CreatedAt = dto.CreatedAt.DateTime,
            ModifiedAt = dto.ModifiedAt.DateTime
        };
        
        return production;
    }
    
    #endregion
    
    #region DTOs
    
    private class ProductionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }
    
    private class CreateProductionDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
    
    private class UpdateProductionDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
    
    #endregion
}