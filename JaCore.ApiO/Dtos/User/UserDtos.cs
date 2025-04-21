// DTOs/UserDtos.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.DTOs;

// DTO for displaying user information (list or detail)
public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsActive,
    DateTime CreatedAt,
    IList<string> Roles);

// DTO for updating user information (Admin or Self)
public record UpdateUserDto(
    [Required, MinLength(2)] string FirstName,
    [Required, MinLength(2)] string LastName,
    [Required, EmailAddress] string Email, // Maybe restrict changing email easily
    bool? IsActive = null, // Allow admin to activate/deactivate
    List<string>? Roles = null); // Allow admin to change roles

// DTO specifically for admin role updates
public record UpdateUserRolesDto(
    [Required] List<string> Roles);
