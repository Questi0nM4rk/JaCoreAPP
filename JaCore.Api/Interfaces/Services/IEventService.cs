using JaCore.Api.Dtos.Common;
using JaCore.Api.Dtos.Device;
using System.Threading.Tasks;

namespace JaCore.Api.Interfaces.Services;

/// <summary>
/// Interface for the service managing Event entities.
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Gets all events associated with a specific DeviceCard asynchronously.
    /// </summary>
    /// <param name="deviceCardId">The ID of the device card.</param>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>A paginated list of Event DTOs.</returns>
    Task<PaginatedListDto<EventDto>> GetByDeviceCardIdAsync(int deviceCardId, int pageNumber, int pageSize);

    /// <summary>
    /// Gets a specific event by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the event.</param>
    /// <returns>The Event DTO, or null if not found.</returns>
    Task<EventDto?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new event asynchronously.
    /// The DeviceCardId must be specified in the CreateEventDto.
    /// </summary>
    /// <param name="createDto">The DTO containing data for the new event.</param>
    /// <returns>The created Event DTO.</returns>
    /// <exception cref="ArgumentException">Thrown if referenced DeviceCardId doesn't exist or validation fails.</exception>
    Task<EventDto> CreateAsync(CreateEventDto createDto);

    /// <summary>
    /// Updates an existing event asynchronously.
    /// </summary>
    /// <param name="id">The ID of the event to update.</param>
    /// <param name="updateDto">The DTO containing the updated data.</param>
    /// <returns>True if the update was successful, false if the event was not found.</returns>
    /// <exception cref="ArgumentException">Thrown on validation errors.</exception>
    Task<bool> UpdateAsync(int id, UpdateEventDto updateDto);

    /// <summary>
    /// Deletes an event asynchronously.
    /// </summary>
    /// <param name="id">The ID of the event to delete.</param>
    /// <returns>True if the deletion was successful, false if the event was not found.</returns>
    Task<bool> DeleteAsync(int id);
} 