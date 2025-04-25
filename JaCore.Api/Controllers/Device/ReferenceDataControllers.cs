using JaCore.Api.DTOs.Device;
using JaCore.Api.Services.Device;
using JaCore.Api.Helpers;
using JaCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JaCore.Api.Controllers.Device;

// --- Categories Controller ---
[ApiController]
[ApiVersion(ApiConstants.Versions.V1_0_String)]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;
    public CategoriesController(ICategoryService service) { _service = service; }

    [HttpGet(ApiConstants.CategoryRoutes.GetAll)]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<CategoryReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken) => Ok(await _service.GetAllCategoriesAsync(cancellationToken));

    [HttpGet(ApiConstants.CategoryRoutes.GetById)]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CategoryReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetCategoryByIdAsync(id, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost(ApiConstants.CategoryRoutes.Create)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    [ProducesResponseType(typeof(CategoryReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.CreateCategoryAsync(dto, cancellationToken);
        if (result == null) return BadRequest(new { message = "Failed to create category. Name might already exist." });
        return CreatedAtAction(nameof(GetCategoryById), new { id = result.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() }, result);
    }

    [HttpPut(ApiConstants.CategoryRoutes.Update)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryUpdateDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var success = await _service.UpdateCategoryAsync(id, dto, cancellationToken);
        if (!success) return NotFound(new { message = $"Category update failed for ID {id}. Resource not found or name conflict occurred." });
        return NoContent();
    }

    [HttpDelete(ApiConstants.CategoryRoutes.Delete)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        var success = await _service.DeleteCategoryAsync(id, cancellationToken);
        return success ? NoContent() : NotFound();
    }
}

// --- Suppliers Controller ---
[ApiController]
[ApiVersion(ApiConstants.Versions.V1_0_String)]
[Produces("application/json")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _service;
    public SuppliersController(ISupplierService service) { _service = service; }

    [HttpGet(ApiConstants.SupplierRoutes.GetAll)]
    [Authorize(Roles = $"{RoleConstants.Roles.Admin},{RoleConstants.Roles.User}")]
    [ProducesResponseType(typeof(IEnumerable<SupplierReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuppliers(CancellationToken cancellationToken) => Ok(await _service.GetAllSuppliersAsync(cancellationToken));

    [HttpGet(ApiConstants.SupplierRoutes.GetById)]
    [Authorize(Roles = $"{RoleConstants.Roles.Admin},{RoleConstants.Roles.User}")]
    [ProducesResponseType(typeof(SupplierReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSupplierById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetSupplierByIdAsync(id, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost(ApiConstants.SupplierRoutes.Create)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    [ProducesResponseType(typeof(SupplierReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSupplier([FromBody] SupplierCreateDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.CreateSupplierAsync(dto, cancellationToken);
        if (result == null) return BadRequest(new { message = "Failed to create supplier." });
        return CreatedAtAction(nameof(GetSupplierById), new { id = result.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() }, result);
    }

    [HttpPut(ApiConstants.SupplierRoutes.Update)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplier(Guid id, [FromBody] SupplierUpdateDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var success = await _service.UpdateSupplierAsync(id, dto, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete(ApiConstants.SupplierRoutes.Delete)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSupplier(Guid id, CancellationToken cancellationToken)
    {
        var success = await _service.DeleteSupplierAsync(id, cancellationToken);
        return success ? NoContent() : NotFound();
    }
}

// --- Services Controller ---
[ApiController]
[ApiVersion(ApiConstants.Versions.V1_0_String)]
[Produces("application/json")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly IServiceService _service;
    public ServicesController(IServiceService service) { _service = service; }

    [HttpGet(ApiConstants.ServiceRoutes.GetAll)]
    [Authorize(Roles = $"{RoleConstants.Roles.Admin},{RoleConstants.Roles.User}")]
    [ProducesResponseType(typeof(IEnumerable<ServiceReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServices(CancellationToken cancellationToken) => Ok(await _service.GetAllServicesAsync(cancellationToken));

    [HttpGet(ApiConstants.ServiceRoutes.GetById)]
    [Authorize(Roles = $"{RoleConstants.Roles.Admin},{RoleConstants.Roles.User}")]
    [ProducesResponseType(typeof(ServiceReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetServiceById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetServiceByIdAsync(id, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost(ApiConstants.ServiceRoutes.Create)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    [ProducesResponseType(typeof(ServiceReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateService([FromBody] ServiceCreateDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.CreateServiceAsync(dto, cancellationToken);
        if (result == null) return BadRequest(new { message = "Failed to create service." });
        return CreatedAtAction(nameof(GetServiceById), new { id = result.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() }, result);
    }

    [HttpPut(ApiConstants.ServiceRoutes.Update)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateService(Guid id, [FromBody] ServiceUpdateDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var success = await _service.UpdateServiceAsync(id, dto, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete(ApiConstants.ServiceRoutes.Delete)]
    [Authorize(Roles = RoleConstants.Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteService(Guid id, CancellationToken cancellationToken)
    {
        var success = await _service.DeleteServiceAsync(id, cancellationToken);
        return success ? NoContent() : NotFound();
    }
} 