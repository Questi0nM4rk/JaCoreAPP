using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.DTOs.Users;

public record UpdateUserDto(
    [Required, MinLength(2)] string FirstName,
    [Required, MinLength(2)] string LastName,
    [Required, EmailAddress] string Email,
    bool? IsActive = null, // Allow admin to modify active status
    List<string>? Roles = null // Allow admin to modify roles via this DTO
);

