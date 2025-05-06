using JaCore.Api.Data;
using JaCore.Api.Services.Abstractions.Auth;
using JaCore.Api.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JaCore.Api.Services.Repositories.Auth;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RefreshTokenRepository> _logger;

    public RefreshTokenRepository(ApplicationDbContext context, ILogger<RefreshTokenRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        _logger.LogDebug("Added RefreshToken entity for user {UserId}, pending save.", refreshToken.UserId);
        // SaveChangesAsync called by service/Unit of Work
    }

    public Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        // Ensure the entity is tracked and marked as modified if necessary
        _context.Entry(refreshToken).State = EntityState.Modified;
        // Or just _context.RefreshTokens.Update(refreshToken); if you prefer explicit call
        _logger.LogDebug("Updated RefreshToken entity {TokenId} for user {UserId}, pending save.", refreshToken.Id, refreshToken.UserId);
        return Task.CompletedTask; // EF Core change tracking is synchronous
    }

    public async Task<List<RefreshToken>> GetValidTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Retrieve tokens that *could* be valid candidates for hash comparison by the service
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId &&
                         rt.ExpiryDate >= DateTime.UtcNow &&
                         !rt.IsRevoked &&
                         !rt.IsUsed)
            .AsNoTracking() // Use NoTracking if the repo method doesn't intend to update the retrieved tokens
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {Count} potentially valid refresh tokens for user {UserId}.", tokens.Count, userId);
        return tokens;
    }

    public void Remove(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Remove(refreshToken);
        _logger.LogDebug("Removed RefreshToken entity {TokenId} for user {UserId}, pending save.", refreshToken.Id, refreshToken.UserId);
        // SaveChangesAsync called by service/Unit of Work
    }
}
