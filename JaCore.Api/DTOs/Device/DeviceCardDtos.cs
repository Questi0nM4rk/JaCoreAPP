using JaCore.Common.Device;
using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.DTOs.Device;

// --- DeviceCard DTOs ---

public record DeviceCardReadDto(
    Guid Id,
    Guid DeviceId,
    string Location,
    string? AssignedUser,
    DateTimeOffset LastSeenAt,
    DeviceDataState DataState,
    DeviceOperationalState OperationalState,
    string? PropertiesJson, // Keep as string for now
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt
);

public record DeviceCardCreateDto(
    [Required] Guid DeviceId,
    [Required] [StringLength(150)] string Location,
    [StringLength(100)] string? AssignedUser,
    // LastSeenAt, States, Timestamps are typically set by the system
    string? PropertiesJson // Allow setting initial properties
);

public record DeviceCardUpdateDto(
    [Required] [StringLength(150)] string Location,
    [StringLength(100)] string? AssignedUser,
    // DeviceId should not be changed
    // States might be updatable through specific actions, not general update
    string? PropertiesJson
); 