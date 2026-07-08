using Domain.Dtos.Users;
using Domain.Interfaces.Users;

namespace Application.Services.Users
{
    internal class CurrentUser : ICurrentUser
    {
        public ApplicationUserDto? Info { get; set; }
    }
}
