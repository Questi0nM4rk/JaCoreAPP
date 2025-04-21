using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;
using JaCore.Api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // For DbUpdateConcurrencyException

namespace JaCore.Api.Controllers.Device;

/// <summary>
/// API Controller for managing device services.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
[Authorize] // Require authentication for all actions
public class ServiceController : ControllerBase
{
    private readonly IServiceService _serviceService;
    private readonly ILogger<ServiceController> _logger;

    /// <summary>
    /// Initializes a new instance of the ServiceController.
    /// </summary>
    /// <param name="serviceService">The service service instance.</param>
    /// <param name="logger">Logger instance.</param>
    public ServiceController(IServiceService serviceService, ILogger<ServiceController> logger)
    {
        _serviceService = serviceService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all services with pagination.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>A paginated list of service DTOs.</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Debug,Management,User")] // All roles can view services
    [ProducesResponseType(typeof(PaginatedListDto<ServiceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedListDto<ServiceDto>>> GetAll(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        _logger.LogInformation("Getting all services with pageNumber: {PageNumber}, pageSize: {PageSize}", pageNumber, pageSize);
        var services = await _serviceService.GetAllAsync(pageNumber, pageSize);
        return Ok(services);
    }
    
    /// <summary>
    /// Gets a specific service by ID.
    /// </summary>
    /// <param name="id">Service ID.</param>
    /// <returns>Service DTO.</returns>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Debug,Management,User")] // All roles can view service details
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceDto>> GetById(int id)
    {
        var service = await _serviceService.GetByIdAsync(id);
        if (service == null)
        {
            return NotFound();
        }
        return Ok(service);
    }
    
    /// <summary>
    /// Creates a new service.
    /// </summary>
    /// <param name="createDto">Service creation data.</param>
    /// <returns>Created service DTO.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Debug,Management")] // Only admin, debug, and management can create
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceDto>> Create([FromBody] CreateServiceDto createDto)
    {
        try
        {
            var createdService = await _serviceService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = createdService.Id }, createdService);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(ex.ParamName ?? "Error", ex.Message);
            return BadRequest(ModelState);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error creating service.");
             return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while creating the service.");
        }
    }
    
    /// <summary>
    /// Updates an existing service.
    /// </summary>
    /// <param name="id">Service ID.</param>
    /// <param name="updateDto">Updated service data.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Debug,Management")] // Only admin, debug, and management can update
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceDto updateDto)
    {
         try
        {
            var success = await _serviceService.UpdateAsync(id, updateDto);
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
             _logger.LogWarning(ex, "Concurrency error updating service {ServiceId}.", id);
             return Conflict("The service was modified by another user. Please refresh and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service {ServiceId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while updating the service.");
        }
    }
    
    /// <summary>
    /// Deletes a service.
    /// </summary>
    /// <param name="id">Service ID to delete.</param>
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
            var success = await _serviceService.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (InvalidOperationException ex) // Catch potential validation errors from service
        {
            _logger.LogWarning("Validation error deleting service {ServiceId}: {ErrorMessage}", id, ex.Message);
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error", 
                Detail = ex.Message 
            });
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error deleting service {ServiceId}.", id);
             return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while deleting the service.");
        }
    }
}