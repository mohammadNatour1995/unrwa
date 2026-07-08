using Microsoft.EntityFrameworkCore;
using Domain.Entities.Auth;
using Domain.Interfaces.Auth;

namespace Infrastructure.Persistence.Repositories.Auth;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<RefreshToken?> FindByTokenAsync(string token)
        => _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);

    public Task CreateAsync(RefreshToken token)
    {
        _context.RefreshTokens.Add(token);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(RefreshToken token)
    {
        _context.RefreshTokens.Update(token);
        return Task.CompletedTask;
    }

    public async Task RevokeAllForUserAsync(string userId, string reason = "Superseded by new login")
    {
        var active = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        if (active.Count == 0) return;

        var now = DateTime.UtcNow;
        foreach (var token in active)
        {
            token.IsRevoked = true;
            token.RevokedDate = now;
            token.ReasonRevoked = reason;
        }
    }
}
