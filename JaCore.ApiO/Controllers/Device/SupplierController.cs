using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // For DbUpdateConcurrencyException

namespace JaCore.Api.Controllers.Device;

/// <summary>
/// API Controller for managing device suppliers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly ILogger<SupplierController> _logger;

    /// <summary>
    /// Initializes a new instance of the SupplierController.
    /// </summary>
    /// <param name="supplierService">The supplier service instance.</param>
    /// <param name="logger">Logger instance.</param>
    public SupplierController(ISupplierService supplierService, ILogger<SupplierController> logger)
    {
        _supplierService = supplierService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all suppliers with pagination.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>A paginated list of supplier DTOs.</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Debug,Management,User")]
    [ProducesResponseType(typeof(PaginatedListDto<SupplierDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedListDto<SupplierDto>>> GetAll(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;
        
        var suppliers = await _supplierService.GetAllAsync(pageNumber, pageSize);
        return Ok(suppliers);
    }
    
    /// <summary>
    /// Gets a specific supplier by ID.
    /// </summary>
    /// <param name="id">Supplier ID.</param>
    /// <returns>Supplier DTO.</returns>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Debug,Management,User")]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierDto>> GetById(int id)
    {
        var supplier = await _supplierService.GetByIdAsync(id);
        if (supplier == null)
        {
            return NotFound();
        }
        return Ok(supplier);
    }
    
    /// <summary>
    /// Creates a new supplier.
    /// </summary>
    /// <param name="createDto">Supplier creation data.</param>
    /// <returns>Created supplier DTO.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Debug,Management")]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SupplierDto>> Create([FromBody] CreateSupplierDto createDto)
    {
        try
        {
            var createdSupplier = await _supplierService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = createdSupplier.Id }, createdSupplier);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(ex.ParamName ?? "Error", ex.Message);
            return BadRequest(ModelState);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error creating supplier.");
             return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while creating the supplier.");
        }
    }
    
    /// <summary>
    /// Updates an existing supplier.
    /// </summary>
    /// <param name="id">Supplier ID.</param>
    /// <param name="updateDto">Updated supplier data.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Debug,Management")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierDto updateDto)
    {
         try
        {
            var success = await _supplierService.UpdateAsync(id, updateDto);
            if (!success)
            {
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
             _logger.LogWarning(ex, "Concurrency error updating supplier {SupplierId}.", id);
             return Conflict("The supplier was modified by another user. Please refresh and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating supplier {SupplierId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while updating the supplier.");
        }
    }
    
    /// <summary>
    /// Deletes a supplier.
    /// </summary>
    /// <param name="id">Supplier ID to delete.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Debug")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id)
    {
         try
        {
            var success = await _supplierService.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (InvalidOperationException ex) // Catch potential validation errors from service
        {
            _logger.LogWarning("Validation error deleting supplier {SupplierId}: {ErrorMessage}", id, ex.Message);
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error", 
                Detail = ex.Message 
            });
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error deleting supplier {SupplierId}.", id);
             return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while deleting the supplier.");
        }
    }
} 