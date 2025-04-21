using System.Net;
using JaCore.Api.Data;
using JaCore.Api.Models.Device;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;

namespace JaCore.Api.Controllers.Device;

/// <summary>
/// API Controller for managing Device Cards, linked to Devices.
/// </summary>
[ApiController]
// Decide on routing strategy:
// Option 1: Flat route /api/DeviceCard (used below)
// Option 2: Nested route (requires adjustments) e.g., /api/Device/{deviceId}/DeviceCard
[Route("api/[controller]")] 
[Authorize] // Require authentication for all actions
public class DeviceCardController : ControllerBase
{
    // Remove direct DbContext dependency
    // private readonly ApplicationDbContext _context;
    private readonly IDeviceCardService _deviceCardService;
    private readonly ILogger<DeviceCardController> _logger;

    /// <summary>
    /// Initializes a new instance of the DeviceCardController.
    /// </summary>
    /// <param name="deviceCardService">The device card service instance.</param>
    /// <param name="logger">Logger instance.</param>
    public DeviceCardController(IDeviceCardService deviceCardService, ILogger<DeviceCardController> logger)
    {
        // _context = context; // Removed
        _deviceCardService = deviceCardService;
        _logger = logger;
    }

    // Removed GetAll - Cards should be retrieved in context of a Device.
    
    /// <summary>
    /// Gets the device card associated with a specific Device ID.
    /// </summary>
    /// <param name="deviceId">The ID of the parent Device.</param>
    /// <returns>Device card details.</returns>
    // Changed route to fetch based on DeviceId
    [HttpGet("ByDevice/{deviceId:int}")] // Route: GET /api/DeviceCard/ByDevice/{deviceId}
    [Authorize(Roles = "Admin,Debug,Management,User")]
    [ProducesResponseType(typeof(DeviceCardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeviceCardDto>> GetByDeviceId(int deviceId)
    {
        var deviceCard = await _deviceCardService.GetByDeviceIdAsync(deviceId);
        if (deviceCard == null)
        {
            _logger.LogInformation("DeviceCard not found for DeviceId: {DeviceId}", deviceId);
            return NotFound();
        }
        return Ok(deviceCard);
    }

    /// <summary>
    /// Gets a specific device card by its own ID.
    /// (Optional: Keep if direct access by card ID is needed)
    /// </summary>
    /// <param name="id">Device card ID.</param>
    /// <returns>Device card details.</returns>
    [HttpGet("{id:int}")] // Route: GET /api/DeviceCard/{id}
    [Authorize(Roles = "Admin,Debug,Management,User")]
    [ProducesResponseType(typeof(DeviceCardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeviceCardDto>> GetById(int id)
    {
        var deviceCard = await _deviceCardService.GetByIdAsync(id);
        if (deviceCard == null)
        {
             _logger.LogInformation("DeviceCard not found for Id: {DeviceCardId}", id);
            return NotFound();
        }
        return Ok(deviceCard);
    }
    
    /// <summary>
    /// Creates a new device card for a specific Device.
    /// </summary>
    /// <param name="deviceId">The ID of the Device to link the card to.</param>
    /// <param name="createDto">Device card creation data.</param>
    /// <returns>Created device card.</returns>
    // Changed route to include deviceId
    [HttpPost("ForDevice/{deviceId:int}")] // Route: POST /api/DeviceCard/ForDevice/{deviceId}
    [Authorize(Roles = "Admin,Debug,Management")]
    [ProducesResponseType(typeof(DeviceCardDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] // If deviceId doesn't exist
    [ProducesResponseType(StatusCodes.Status409Conflict)] // If card already exists for device
    public async Task<ActionResult<DeviceCardDto>> Create(int deviceId, [FromBody] CreateDeviceCardDto createDto)
    {
        try
        {
            var createdCard = await _deviceCardService.CreateAsync(deviceId, createDto);
            // Return 201 Created, pointing to the GetById endpoint for the new card
            return CreatedAtAction(nameof(GetById), new { id = createdCard.Id }, createdCard);
        }
        catch (ArgumentException ex) // Handles validation errors (e.g., device not found)
        {
            _logger.LogWarning("Validation error creating DeviceCard for DeviceId {DeviceId}: {ErrorMessage}", deviceId, ex.Message);
            // Check if it's a 'not found' error based on ParamName or specific exception type if available
            if (ex.ParamName == "deviceId") 
            {
                 return NotFound(new ProblemDetails { Title = "Not Found", Detail = ex.Message });
            }
            ModelState.AddModelError(ex.ParamName ?? "Error", ex.Message);
            return BadRequest(ModelState);
        }
        catch (InvalidOperationException ex) // Handles conflict (e.g., card already exists)
        {
             _logger.LogWarning("Conflict creating DeviceCard for DeviceId {DeviceId}: {ErrorMessage}", deviceId, ex.Message);
             return Conflict(new ProblemDetails { Title = "Conflict", Detail = ex.Message });
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error creating DeviceCard for DeviceId {DeviceId}", deviceId);
             return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while creating the device card.");
        }
    }
    
    /// <summary>
    /// Updates an existing device card by its ID.
    /// </summary>
    /// <param name="id">Device card ID.</param>
    /// <param name="updateDto">Updated device card data.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id:int}")] // Route: PUT /api/DeviceCard/{id}
    [Authorize(Roles = "Admin,Debug,Management")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDeviceCardDto updateDto)
    {
       try
        {
            var success = await _deviceCardService.UpdateAsync(id, updateDto);
            if (!success)
            {
                _logger.LogInformation("DeviceCard not found for update with Id: {DeviceCardId}", id);
                return NotFound(); // Card with 'id' not found
            }
            return NoContent();
        }
        catch (ArgumentException ex)
        {
             _logger.LogWarning("Validation error updating DeviceCard {DeviceCardId}: {ErrorMessage}", id, ex.Message);
            ModelState.AddModelError(ex.ParamName ?? "Error", ex.Message);
            return BadRequest(ModelState);
        }
        // Add concurrency exception handling if needed/service throws it
        // catch (DbUpdateConcurrencyException ex) { ... return Conflict(); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating DeviceCard {DeviceCardId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred updating the device card.");
        }
    }
    
    /// <summary>
    /// Deletes a device card by its ID.
    /// </summary>
    /// <param name="id">Device card ID to delete.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:int}")] // Route: DELETE /api/DeviceCard/{id}
    [Authorize(Roles = "Admin,Debug")] // More restrictive potentially
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            // Service now handles unlinking from Device
            var success = await _deviceCardService.DeleteAsync(id);
            if (!success)
            {
                _logger.LogInformation("DeviceCard not found for deletion with Id: {DeviceCardId}", id);
                return NotFound(); // Card with 'id' not found
            }
            return NoContent();
        }
        // Add InvalidOperationException catch if service throws specific errors (e.g., cannot delete if events exist)
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error deleting DeviceCard {DeviceCardId}", id);
             return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred deleting the device card.");
        }
    }
    
    // Removed DeviceCardExists - Service layer handles this.
} 