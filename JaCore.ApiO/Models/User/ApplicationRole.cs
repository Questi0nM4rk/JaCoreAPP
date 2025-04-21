// Entities/ApplicationRole.cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace JaCore.Api.Models.User;

public class ApplicationRole : IdentityRole<Guid> // Using Guid as the key type
{
    [MaxLength(250)]
    public string? Description { get; set; }

    public ApplicationRole() : base() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
