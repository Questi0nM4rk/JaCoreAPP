using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.Entities.Identity;

public class ApplicationRole : IdentityRole<Guid>
{
    [MaxLength(250)]
    public string? Description { get; set; }

    public ApplicationRole() : base() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
