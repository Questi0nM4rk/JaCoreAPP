using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Interfaces.Services;
using JaCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Added for DbUpdateConcurrencyException

namespace JaCore.Api.Controllers.Device;

/// <summary>
/// API Controller for managing device categories.
/// </summary>
[ApiController]
[Route(ApiConstants.ApiVersionPrefix + "/[controller]")]
[Authorize] // Require authentication for all actions
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoryController> _logger; // Add logger for controller-level events

    /// <summary>
    /// Initializes a new instance of the CategoryController.
    /// </summary>
    /// <param name="categoryService">The category service instance.</param>
    /// <param name="logger">Logger instance.</param>
    public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all categories with pagination.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>A paginated list of category DTOs.</returns>
    [HttpGet(ApiConstants.Routes.GetAll)]
    [Authorize(Policy = ApiConstants.Policies.ReadOnly)]
    [ProducesResponseType(typeof(PaginatedListDto<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedListDto<CategoryDto>>> GetAll(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var categories = await _categoryService.GetAllAsync(pageNumber, pageSize);
        return Ok(categories);
    }
    
    /// <summary>
    /// Gets a specific category by ID.
    /// </summary>
    /// <param name="id">Category ID.</param>
    /// <returns>Category DTO.</returns>
    [HttpGet(ApiConstants.Routes.GetById)]
    [Authorize(Policy = ApiConstants.Policies.ReadOnly)]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return Ok(category);
    }
    
    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="createDto">Category creation data.</param>
    /// <returns>Created category DTO.</returns>
    [HttpPost(ApiConstants.Routes.Create)]
    [Authorize(Policy = ApiConstants.Policies.ReadWrite)]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto createDto)
    {
        try
        {
            var createdCategory = await _categoryService.CreateAsync(createDto);
            // Return 201 Created with location header and the created resource
            return CreatedAtAction(nameof(GetById), new { id = createdCategory.Id }, createdCategory);
        }
        catch (ArgumentException ex)
        {
            // Handle validation errors from the service layer
            ModelState.AddModelError(ex.ParamName ?? "Error", ex.Message);
            return BadRequest(ModelState);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error creating category.");
             return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while creating the category.");
        }
    }
    
    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="id">Category ID.</param>
    /// <param name="updateDto">Updated category data.</param>
    /// <returns>No content on success.</returns>
    [HttpPut(ApiConstants.Routes.Update)]
    [Authorize(Policy = ApiConstants.Policies.ReadWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto updateDto)
    {
        try
        {
            var success = await _categoryService.UpdateAsync(id, updateDto);
            if (!success)
            {
                // Service layer returns false if not found (or potentially other handled errors)
                return NotFound();
            }
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(ex.ParamName ?? "Error", ex.Message);
            return BadRequest(ModelState);
        }
        catch (DbUpdateConcurrencyException ex)
        {
             _logger.LogWarning(ex, "Concurrency error updating category {CategoryId}.", id);
             // Let the global exception handler handle this, or return Conflict
             return Conflict("The category was modified by another user. Please refresh and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while updating the category.");
        }
    }
    
    /// <summary>
    /// Deletes a category.
    /// </summary>
    /// <param name="id">Category ID to delete.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete(ApiConstants.Routes.Delete)]
    [Authorize(Policy = ApiConstants.Policies.FullAccess)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // For validation errors like linked devices
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _categoryService.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (InvalidOperationException ex) // Catch specific exception for linked devices
        {
            _logger.LogWarning("Validation error deleting category {CategoryId}: {ErrorMessage}", id, ex.Message);
            // Return 400 Bad Request with details
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error", 
                Detail = ex.Message // Get message from the service exception
            });
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error deleting category {CategoryId}.", id);
             return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while deleting the category.");
        }
    }
}