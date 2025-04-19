using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Interfaces.Services;
using JaCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JaCore.Api.Controllers.Device;

/// <summary>
/// API Controller for managing devices.
/// </summary>
[ApiController]
[Route(ApiConstants.ApiRoutePrefix + "/[controller]")]
[Authorize] // Require authentication for all actions
public class DeviceController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly ILogger<DeviceController> _logger;

    /// <summary>
    /// Initializes a new instance of the DeviceController.
    /// </summary>
    /// <param name="deviceService">The device service instance.</param>
    /// <param name="logger">Logger instance.</param>
    public DeviceController(IDeviceService deviceService, ILogger<DeviceController> logger)
    {
        _deviceService = deviceService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all devices with pagination, sorted by last modified descending by default.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>A paginated list of device DTOs.</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Debug,Management,User")] // All roles can view devices
    [ProducesResponseType(typeof(PaginatedListDto<DeviceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedListDto<DeviceDto>>> GetAll(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var devices = await _deviceService.GetAllAsync(pageNumber, pageSize);
        return Ok(devices);
    }
    
    /// <summary>
    /// Gets a specific device by ID.
    /// </summary>
    /// <param name="id">Device ID.</param>
    /// <returns>Device DTO.</returns>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Debug,Management,User")] // All roles can view device details
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeviceDto>> GetById(int id)
    {
        var device = await _deviceService.GetByIdAsync(id);
        if (device == null)
        {
            return NotFound();
        }
        return Ok(device);
    }
    
    /// <summary>
    /// Creates a new device.
    /// </summary>
    /// <param name="createDto">Device creation data.</param>
    /// <returns>Created device DTO.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Debug,Management")] // Only admin, debug, and management can create
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DeviceDto>> Create([FromBody] CreateDeviceDto createDto)
    {
        try
        {
            var createdDevice = await _deviceService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = createdDevice.Id }, createdDevice);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(ex.ParamName ?? "Error", ex.Message);
            return BadRequest(ModelState);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error creating device.");
             return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while creating the device.");
        }
    }
    
    /// <summary>
    /// Updates an existing device.
    /// </summary>
    /// <param name="id">Device ID.</param>
    /// <param name="updateDto">Updated device data.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Debug,Management")] // Only admin, debug, and management can update
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDeviceDto updateDto)
    {
        try
        {
            var success = await _deviceService.UpdateAsync(id, updateDto);
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
             _logger.LogWarning(ex, "Concurrency error updating device {DeviceId}.", id);
             return Conflict("The device was modified by another user. Please refresh and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating device {DeviceId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while updating the device.");
        }
    }
    
    /// <summary>
    /// Deletes a device.
    /// </summary>
    /// <param name="id">Device ID to delete.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Debug")] // Only admin and debug can delete
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _deviceService.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (InvalidOperationException ex) // Catch potential validation errors from service
        {
            _logger.LogWarning("Validation error deleting device {DeviceId}: {ErrorMessage}", id, ex.Message);
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error", 
                Detail = ex.Message 
            });
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error deleting device {DeviceId}.", id);
             return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while deleting the device.");
        }
    }
} 