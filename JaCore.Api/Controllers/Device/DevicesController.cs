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
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    public DevicesController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpGet(ApiConstants.DeviceRoutes.GetAll)]
    [ProducesResponseType(typeof(IEnumerable<DeviceDto>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RoleConstants.Roles.Admin},{RoleConstants.Roles.User}")]
    public async Task<IActionResult> GetDevices(CancellationToken cancellationToken)
    {
        var devices = await _deviceService.GetAllDevicesAsync(cancellationToken);
        return Ok(devices);
    }

    [HttpGet(ApiConstants.DeviceRoutes.GetById)]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{RoleConstants.Roles.Admin},{RoleConstants.Roles.User}")]
    public async Task<IActionResult> GetDeviceById(Guid id, CancellationToken cancellationToken)
    {
        var device = await _deviceService.GetDeviceByIdAsync(id, cancellationToken);
        return device == null ? NotFound() : Ok(device);
    }

    [HttpGet(ApiConstants.DeviceRoutes.GetBySerial)]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{RoleConstants.Roles.Admin},{RoleConstants.Roles.User}")]
    public async Task<IActionResult> GetDeviceBySerialNumber(string serialNumber, CancellationToken cancellationToken)
    {
        var device = await _deviceService.GetDeviceBySerialNumberAsync(serialNumber, cancellationToken);
        return device == null ? NotFound() : Ok(device);
    }

    [HttpPost(ApiConstants.DeviceRoutes.Create)]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    public async Task<IActionResult> CreateDevice([FromBody] DeviceCreateDto createDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var createdDevice = await _deviceService.CreateDeviceAsync(createDto, cancellationToken);
        if (createdDevice == null)
        {
            return BadRequest(new { message = "Failed to create device. Serial number might already exist or invalid data provided." });
        }
        return CreatedAtAction(nameof(GetDeviceById), new { id = createdDevice.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() }, createdDevice);
    }

    [HttpPut(ApiConstants.DeviceRoutes.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    public async Task<IActionResult> UpdateDevice(Guid id, [FromBody] DeviceUpdateDto updateDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var success = await _deviceService.UpdateDeviceAsync(id, updateDto, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete(ApiConstants.DeviceRoutes.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    public async Task<IActionResult> DeleteDevice(Guid id, CancellationToken cancellationToken)
    {
        var success = await _deviceService.DeleteDeviceAsync(id, cancellationToken);
        return success ? NoContent() : NotFound();
    }
} 