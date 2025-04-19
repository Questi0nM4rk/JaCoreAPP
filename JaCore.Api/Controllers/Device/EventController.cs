using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JaCore.Api.Controllers.Device;

/// <summary>
/// API Controller for managing device events, linked to Device Cards.
/// </summary>
[ApiController]
// Consider a nested route like /api/DeviceCard/{deviceCardId}/Events
// Or keep it flat: /api/Event and filter by query param
[Route("api/[controller]")] // Route: /api/Event
[Authorize] 
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly ILogger<EventController> _logger;

    public EventController(IEventService eventService, ILogger<EventController> logger)
    {
        _eventService = eventService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all events for a specific Device Card with pagination.
    /// </summary>
    /// <param name="deviceCardId">The ID of the Device Card.</param>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>A paginated list of event DTOs for the specified card.</returns>
    [HttpGet] // Route: GET /api/Event?deviceCardId=X
    [Authorize(Roles = "Admin,Debug,Management,User")]
    [ProducesResponseType(typeof(PaginatedListDto<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // If deviceCardId is invalid
    public async Task<ActionResult<PaginatedListDto<EventDto>>> GetByDeviceCardId(
        [FromQuery] int deviceCardId,
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20)
    {
        if (deviceCardId <= 0)
        {
            return BadRequest("A valid DeviceCardId must be provided.");
        }
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        // Service checks if card exists and returns events or empty list
        var events = await _eventService.GetByDeviceCardIdAsync(deviceCardId, pageNumber, pageSize);
        return Ok(events);
    }
    
    /// <summary>
    /// Gets a specific event by its own ID.
    /// </summary>
    /// <param name="id">Event ID.</param>
    /// <returns>Event DTO.</returns>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Debug,Management,User")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetById(int id)
    {
        var ev = await _eventService.GetByIdAsync(id);
        if (ev == null)
        {
            return NotFound();
        }
        return Ok(ev);
    }
    
    /// <summary>
    /// Creates a new event linked to a Device Card.
    /// </summary>
    /// <param name="createDto">Event creation data (must include DeviceCardId).</param>
    /// <returns>Created event DTO.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Debug,Management")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventDto>> Create([FromBody] CreateEventDto createDto)
    {
        // Validation of DeviceCardId happens in the service layer
        try
        {
            var createdEvent = await _eventService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = createdEvent.Id }, createdEvent);
        }
        catch (ArgumentException ex) // Catch validation errors from service
        {
            ModelState.AddModelError(ex.ParamName ?? "Error", ex.Message);
            return BadRequest(ModelState);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error creating event for DeviceCardId {DeviceCardId}.", createDto.DeviceCardId);
             return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred creating the event.");
        }
    }
    
    /// <summary>
    /// Updates an existing event.
    /// </summary>
    /// <param name="id">Event ID.</param>
    /// <param name="updateDto">Updated event data.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Debug,Management")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEventDto updateDto)
    {
        try
        {
            var success = await _eventService.UpdateAsync(id, updateDto);
            if (!success)
            {
                return NotFound(); // Event with 'id' not found
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
             _logger.LogWarning(ex, "Concurrency error updating event {EventId}.", id);
             return Conflict("The event was modified by another user. Please refresh and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event {EventId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred updating the event.");
        }
    }
    
    /// <summary>
    /// Deletes an event.
    /// </summary>
    /// <param name="id">Event ID to delete.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Debug")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _eventService.DeleteAsync(id);
            if (!success)
            {
                return NotFound(); // Event with 'id' not found
            }
            return NoContent();
        }
        // Removed InvalidOperationException catch unless service throws specific validation errors
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error deleting event {EventId}.", id);
             return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred deleting the event.");
        }
    }
} 