using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Users;

[Table("AspNetRoles")]
public class ApplicationRole : IdentityRole
{
}
