using JaCore.Api.Entities.Auth;
using JaCore.Api.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JaCore.Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Important for Identity schema

        // Configure RefreshToken entity
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.TokenHash).IsRequired();
            entity.Property(rt => rt.ExpiryDate).IsRequired();

            // Relationship: One User can have potentially many RefreshTokens over time
            entity.HasOne(rt => rt.User)
                  .WithMany() // No navigation property on User needed for this side
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade); // If user is deleted, cascade delete their refresh tokens

            // Optional index for performance
            entity.HasIndex(rt => rt.UserId);
        });
    }
}
