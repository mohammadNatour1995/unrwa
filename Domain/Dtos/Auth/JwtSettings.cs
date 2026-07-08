using System.ComponentModel.DataAnnotations;

namespace Domain.Dtos.Auth;

public class JwtSettings
{
    [Required, MinLength(32)]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    [Range(1, 90)]
    public int RefreshTokenExpirationDays { get; set; } = 30;
}