using System.ComponentModel.DataAnnotations;

namespace Domain.Dtos.Auth;

public class RefreshTokenRequestDto
{
    public string? AccessToken { get; set; }

    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
