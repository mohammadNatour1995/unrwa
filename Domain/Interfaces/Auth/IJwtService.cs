using Domain.Dtos.Auth;
using Domain.Entities.Users;

namespace Domain.Interfaces.Auth;

public interface IJwtService
{
    Task<LoginResponseDto> GenerateTokensAsync(ApplicationUser user);
    Task<RefreshTokenResponseDto> RefreshTokenAsync(string? accessToken, string refreshToken);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken, string reason = "Revoked by user");
}
