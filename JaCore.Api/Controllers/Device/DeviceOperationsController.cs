using JaCore.Api.DTOs.Device;
using JaCore.Api.Services.Device;
using JaCore.Api.Helpers;
using JaCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JaCore.Api.Controllers.Device;

[ApiController]
[ApiVersion(ApiConstants.Versions.V1_0_String)]
[Produces("application/json")]
[Authorize]
public class DeviceOperationsController : ControllerBase
{
    private readonly IDeviceOperationService _operationService;

    public DeviceOperationsController(IDeviceOperationService operationService)
    {
        _operationService = operationService;
    }

    [HttpGet(ApiConstants.DeviceOperationRoutes.GetByCardId)]
    [ProducesResponseType(typeof(IEnumerable<DeviceOperationReadDto>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RoleConstants.Roles.Admin},{RoleConstants.Roles.User}")]
    public async Task<IActionResult> GetOperationsByDeviceCardId(Guid deviceCardId, CancellationToken cancellationToken)
    {
        var operations = await _operationService.GetOperationsByDeviceCardIdAsync(deviceCardId, cancellationToken);
        return Ok(operations);
    }

    [HttpGet(ApiConstants.DeviceOperationRoutes.GetById)]
    [ProducesResponseType(typeof(DeviceOperationReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{RoleConstants.Roles.Admin},{RoleConstants.Roles.User}")]
    public async Task<IActionResult> GetOperationById(Guid id, CancellationToken cancellationToken)
    {
        var operationDto = await _operationService.GetOperationByIdAsync(id, cancellationToken);
        return operationDto == null ? NotFound() : Ok(operationDto);
    }

    [HttpPost(ApiConstants.DeviceOperationRoutes.Create)]
    [ProducesResponseType(typeof(DeviceOperationReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    public async Task<IActionResult> CreateOperation([FromBody] DeviceOperationCreateDto createDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var createdOperation = await _operationService.CreateOperationAsync(createDto, cancellationToken);
        if (createdOperation == null)
        {
            return BadRequest(new { message = "Failed to create device operation. Invalid DeviceCard ID or data." });
        }

        return CreatedAtAction(nameof(GetOperationById), new { id = createdOperation.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() }, createdOperation);
    }

    [HttpPut(ApiConstants.DeviceOperationRoutes.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    public async Task<IActionResult> UpdateOperation(Guid id, [FromBody] DeviceOperationUpdateDto updateDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var success = await _operationService.UpdateOperationAsync(id, updateDto, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete(ApiConstants.DeviceOperationRoutes.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    public async Task<IActionResult> DeleteOperation(Guid id, CancellationToken cancellationToken)
    {
        var success = await _operationService.DeleteOperationAsync(id, cancellationToken);
        return success ? NoContent() : NotFound();
    }
} 