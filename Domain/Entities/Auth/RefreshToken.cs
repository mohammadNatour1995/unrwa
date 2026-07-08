using Domain.Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Auth;

[Table("RefreshTokens")]
public class RefreshToken
{
    [Key]
    public string Token { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public DateTime ExpiryDate { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? RevokedDate { get; set; }

    public string? ReplacedByToken { get; set; }

    public string? ReasonRevoked { get; set; }

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;
}
