using JaCore.Api.DTOs.Device;
using JaCore.Api.Services.Abstractions.Device;
using JaCore.Api.Helpers;
using JaCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JaCore.Api.Controllers.Device;

[ApiController]
[ApiVersion(ApiConstants.Versions.VersionString)]
[Produces("application/json")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet(ApiConstants.EventRoutes.GetByCardId)]
    [ProducesResponseType(typeof(IEnumerable<EventReadDto>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RoleConstants.Roles.Admin},{RoleConstants.Roles.User}")]
    public async Task<IActionResult> GetEventsByDeviceCardId(Guid deviceCardId, CancellationToken cancellationToken)
    {
        var events = await _eventService.GetEventsByDeviceCardIdAsync(deviceCardId, cancellationToken);
        return Ok(events);
    }

    [HttpGet(ApiConstants.EventRoutes.GetById)]
    [ProducesResponseType(typeof(EventReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{RoleConstants.Roles.Admin},{RoleConstants.Roles.User}")]
    public async Task<IActionResult> GetEventById(Guid id, CancellationToken cancellationToken)
    {
        var eventDto = await _eventService.GetEventByIdAsync(id, cancellationToken);
        return eventDto == null ? NotFound() : Ok(eventDto);
    }

    [HttpPost(ApiConstants.EventRoutes.Create)]
    [ProducesResponseType(typeof(EventReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    public async Task<IActionResult> CreateEvent([FromBody] EventCreateDto createDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var createdEvent = await _eventService.CreateEventAsync(createDto, cancellationToken);
        if (createdEvent == null)
        {
            return BadRequest(new { message = "Failed to create event. Invalid DeviceCard ID or data." });
        }

        return CreatedAtAction(nameof(GetEventById), new { id = createdEvent.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() }, createdEvent);
    }

    [HttpDelete(ApiConstants.EventRoutes.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    public async Task<IActionResult> DeleteEvent(Guid id, CancellationToken cancellationToken)
    {
        var success = await _eventService.DeleteEventAsync(id, cancellationToken);
        return success ? NoContent() : NotFound();
    }
} 