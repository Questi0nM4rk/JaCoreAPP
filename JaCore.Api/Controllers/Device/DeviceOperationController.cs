using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // For DbUpdateConcurrencyException

namespace JaCore.Api.Controllers.Device;

/// <summary>
/// API Controller for managing device operations, linked to Device Cards.
/// </summary>
[ApiController]
[Route("api/[controller]")] // Base route: /api/DeviceOperation
[Authorize] 
public class DeviceOperationController : ControllerBase
{
    private readonly IDeviceOperationService _operationService;
    private readonly ILogger<DeviceOperationController> _logger;

    public DeviceOperationController(IDeviceOperationService operationService, ILogger<DeviceOperationController> logger)
    {
        _operationService = operationService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all device operations for a specific Device Card with pagination.
    /// </summary>
    /// <param name="deviceCardId">The ID of the Device Card.</param>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>A paginated list of device operation DTOs for the specified card.</returns>
    // Changed route to be nested under DeviceCard conceptually, or use query param
    // Using query param for simplicity here: GET /api/DeviceOperation?deviceCardId=X
    [HttpGet]
    [Authorize(Roles = "Admin,Debug,Management,User")]
    [ProducesResponseType(typeof(PaginatedListDto<DeviceOperationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // If deviceCardId is invalid
    public async Task<ActionResult<PaginatedListDto<DeviceOperationDto>>> GetByDeviceCardId(
        [FromQuery] int deviceCardId, // Get ID from query string
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20)
    {
        if (deviceCardId <= 0)
        {
            return BadRequest("A valid DeviceCardId must be provided.");
        }
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;
        
        // Service layer now handles getting by card ID and checking if card exists
        var operations = await _operationService.GetByDeviceCardIdAsync(deviceCardId, pageNumber, pageSize);
        return Ok(operations); 
    }
    
    /// <summary>
    /// Gets a specific device operation by its own ID.
    /// </summary>
    /// <param name="id">Device operation ID.</param>
    /// <returns>Device operation DTO.</returns>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Debug,Management,User")]
    [ProducesResponseType(typeof(DeviceOperationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeviceOperationDto>> GetById(int id)
    {
        var operation = await _operationService.GetByIdAsync(id);
        if (operation == null)
        {
            return NotFound();
        }
        return Ok(operation);
    }
    
    /// <summary>
    /// Creates a new device operation linked to a Device Card.
    /// </summary>
    /// <param name="createDto">Device operation creation data (must include DeviceCardId).</param>
    /// <returns>Created device operation DTO.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Debug,Management")]
    [ProducesResponseType(typeof(DeviceOperationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DeviceOperationDto>> Create([FromBody] CreateDeviceOperationDto createDto)
    {
        // Validation of DeviceCardId happens in the service layer now
        try
        {
            var createdOperation = await _operationService.CreateAsync(createDto);
            // Return 201 Created with location header pointing to the new resource URL
            return CreatedAtAction(nameof(GetById), new { id = createdOperation.Id }, createdOperation);
        }
        catch (ArgumentException ex) // Catch validation errors from service (e.g., bad DeviceCardId)
        {
            ModelState.AddModelError(ex.ParamName ?? "Error", ex.Message);
            return BadRequest(ModelState);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error creating device operation for DeviceCardId {DeviceCardId}.", createDto.DeviceCardId);
             return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred creating the device operation.");
        }
    }
    
    /// <summary>
    /// Updates an existing device operation.
    /// </summary>
    /// <param name="id">Device operation ID.</param>
    /// <param name="updateDto">Updated device operation data.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Debug,Management")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDeviceOperationDto updateDto)
    {
        try
        {
            var success = await _operationService.UpdateAsync(id, updateDto);
            if (!success)
            {
                return NotFound(); // Operation with 'id' not found
            }
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(ex.ParamName ?? "Error", ex.Message);
            return BadRequest(ModelState);
        }
        catch (DbUpdateConcurrencyException ex) // Keep concurrency check if needed
        {
             _logger.LogWarning(ex, "Concurrency error updating device operation {OperationId}.", id);
             return Conflict("The operation was modified by another user. Please refresh and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating device operation {OperationId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred updating the device operation.");
        }
    }
    
    /// <summary>
    /// Deletes a device operation.
    /// </summary>
    /// <param name="id">Device operation ID to delete.</param>
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
            var success = await _operationService.DeleteAsync(id);
            if (!success)
            {
                return NotFound(); // Operation with 'id' not found
            }
            return NoContent();
        }
        // Removed InvalidOperationException catch, service handles basic not found
        // Add back if service throws specific exceptions for deletion validation
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error deleting device operation {OperationId}.", id);
             return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred deleting the device operation.");
        }
    }
} 