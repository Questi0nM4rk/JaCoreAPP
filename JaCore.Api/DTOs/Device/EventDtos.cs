using JaCore.Common.Device;
using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.DTOs.Device;

// --- Event DTOs ---

public record EventReadDto(
    Guid Id,
    Guid DeviceCardId,
    EventType Type,
    DateTimeOffset Timestamp,
    string Description,
    string? DetailsJson,
    string? TriggeredBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt
);

public record EventCreateDto(
    [Required] Guid DeviceCardId,
    [Required] EventType Type,
    [Required] DateTimeOffset Timestamp,
    [Required] [StringLength(500)] string Description,
    string? DetailsJson,
    [StringLength(100)] string? TriggeredBy
);

// Events are typically immutable once created, so no Update DTO is provided.
// If updates are needed, define EventUpdateDto here. 