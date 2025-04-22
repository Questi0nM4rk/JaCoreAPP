using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.DTOs.Users;

public record UpdateUserRolesDto(
    [Required] List<string> Roles // Specifically for admin role updates
);
