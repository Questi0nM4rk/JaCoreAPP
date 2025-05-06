

namespace JaCore.Api.DTOs.Device;

// --- DeviceOperation DTOs ---

public record DeviceOperationReadDto(
    Guid Id,
    Guid DeviceCardId,
    string OperationType,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    string Status,
    string? Operator,
    string? ResultsJson,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt
);

public record DeviceOperationCreateDto(
    [Required] Guid DeviceCardId,
    [Required] [StringLength(100)] string OperationType,
    [Required] DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    [Required] [StringLength(50)] string Status,
    [StringLength(100)] string? Operator,
    string? ResultsJson
);

public record DeviceOperationUpdateDto(
    // Only allow updating specific fields, e.g., EndTime, Status, Results
    DateTimeOffset? EndTime,
    [Required] [StringLength(50)] string Status,
    string? ResultsJson
    // DeviceCardId, OperationType, StartTime, Operator are likely not updatable after creation
); 