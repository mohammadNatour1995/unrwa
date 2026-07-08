
using Domain.Dtos.Users;

namespace Domain.Dtos.Auth;

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiration { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public ApplicationUserDto User { get; set; } = new();
}
