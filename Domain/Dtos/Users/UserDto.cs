using System.ComponentModel.DataAnnotations;

namespace Domain.Dtos.Users;

public class UserDto
{
    public string? Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 6)]
    public string? Password { get; set; }

    [Required]
    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
