using Domain.Dtos;
using Domain.Dtos.Auth;

namespace Domain.Interfaces.Auth;

public interface IAuthService
{
    Task<BaseResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request);
    Task<BaseResponse<RefreshTokenResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<BaseResponse> LogoutAsync(string refreshToken);
}
