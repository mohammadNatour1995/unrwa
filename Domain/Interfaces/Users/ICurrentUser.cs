using Domain.Dtos.Users;

namespace Domain.Interfaces.Users;

public interface ICurrentUser
{
    public ApplicationUserDto? Info { get; set; }
}
