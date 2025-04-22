using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JaCore.Api.Entities.Identity; // Use correct using

namespace JaCore.Api.Entities.Auth;

public class RefreshToken
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; } // Linked user

    [Required]
    public string TokenHash { get; set; } = string.Empty; // Store a hash of the token

    [Required]
    public DateTime ExpiryDate { get; set; } // When this refresh token expires

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public bool IsRevoked { get; set; } = false; // Flag to invalidate token

    public bool IsUsed { get; set; } = false; // Flag for rotation tracking

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!; // Navigation property
}
