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
public class DeviceCardsController : ControllerBase
{
    private readonly IDeviceCardService _deviceCardService;

    public DeviceCardsController(IDeviceCardService deviceCardService)
    {
        _deviceCardService = deviceCardService;
    }

    [HttpGet(ApiConstants.DeviceCardRoutes.GetAll)]
    [ProducesResponseType(typeof(IEnumerable<DeviceCardReadDto>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RoleConstants.Roles.Admin},{RoleConstants.Roles.User}")]
    public async Task<IActionResult> GetDeviceCards(CancellationToken cancellationToken)
    {
        var cards = await _deviceCardService.GetAllDeviceCardsAsync(cancellationToken);
        return Ok(cards);
    }

    [HttpGet(ApiConstants.DeviceCardRoutes.GetById)]
    [ProducesResponseType(typeof(DeviceCardReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{RoleConstants.Roles.Admin},{RoleConstants.Roles.User}")]
    public async Task<IActionResult> GetDeviceCardById(Guid id, CancellationToken cancellationToken)
    {
        var card = await _deviceCardService.GetDeviceCardByIdAsync(id, cancellationToken);
        return card == null ? NotFound() : Ok(card);
    }

    [HttpGet(ApiConstants.DeviceCardRoutes.GetByDeviceId)]
    [ProducesResponseType(typeof(IEnumerable<DeviceCardReadDto>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RoleConstants.Roles.Admin},{RoleConstants.Roles.User}")]
    public async Task<IActionResult> GetDeviceCardsByDeviceId(Guid deviceId, CancellationToken cancellationToken)
    {
        var cards = await _deviceCardService.GetDeviceCardsByDeviceIdAsync(deviceId, cancellationToken);
        return Ok(cards);
    }

    [HttpPost(ApiConstants.DeviceCardRoutes.Create)]
    [ProducesResponseType(typeof(DeviceCardReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    public async Task<IActionResult> CreateDeviceCard([FromBody] DeviceCardCreateDto createDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var createdCard = await _deviceCardService.CreateDeviceCardAsync(createDto, cancellationToken);
        if (createdCard == null)
        {
            return BadRequest(new { message = "Failed to create device card. Invalid Device ID, card already exists for device, or other validation error." });
        }

        return CreatedAtAction(nameof(GetDeviceCardById), new { id = createdCard.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() }, createdCard);
    }

    [HttpPut(ApiConstants.DeviceCardRoutes.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    public async Task<IActionResult> UpdateDeviceCard(Guid id, [FromBody] DeviceCardUpdateDto updateDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var success = await _deviceCardService.UpdateDeviceCardAsync(id, updateDto, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete(ApiConstants.DeviceCardRoutes.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    public async Task<IActionResult> DeleteDeviceCard(Guid id, CancellationToken cancellationToken)
    {
        var success = await _deviceCardService.DeleteDeviceCardAsync(id, cancellationToken);
        return success ? NoContent() : NotFound();
    }
} 