using JaCore.Api.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JaCore.Api.Services.Abstractions;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task<List<RefreshToken>> GetValidTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    void Remove(RefreshToken refreshToken);
    // Task<RefreshToken?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default); // Alternative find method if needed
}
