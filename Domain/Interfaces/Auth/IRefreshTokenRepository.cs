using Domain.Entities.Auth;

namespace Domain.Interfaces.Auth;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> FindByTokenAsync(string token);
    Task CreateAsync(RefreshToken token);
    Task UpdateAsync(RefreshToken token);
    Task RevokeAllForUserAsync(string userId, string reason = "Superseded by new login");
}
