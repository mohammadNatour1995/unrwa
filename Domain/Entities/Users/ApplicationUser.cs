using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Users;

[Table("AspNetUsers")]
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreateDate { get; set; }
}
