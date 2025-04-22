using System;
using System.Collections.Generic;

namespace JaCore.Api.DTOs.Users;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsActive,
    DateTime CreatedAt,
    IList<string> Roles
);
