using Domain.Entities.Users;

namespace Domain.Records;

public record UserDefinition(
    string Id,
    string UserName,
    string PhoneNumber,
    string Email,
    string PasswordHash,
    string SecurityStamp,
    string ConcurrencyStamp,
    string FullName,
    DateTime CreateDate)
{
    public string NormalizedUserName => UserName.ToUpperInvariant();
    public string NormalizedEmail => Email.ToUpperInvariant();

    public ApplicationUser ToEntity()
    {
        var user = new ApplicationUser
        {
            Id = Id,
            UserName = UserName,
            FullName = FullName,
            NormalizedUserName = NormalizedUserName,
            Email = Email,
            NormalizedEmail = NormalizedEmail,
            EmailConfirmed = true,
            PhoneNumber = PhoneNumber,
            PhoneNumberConfirmed = true,
            AccessFailedCount = 0,
            CreateDate = CreateDate,
            IsActive = true,
            LockoutEnabled = false,
            LockoutEnd = null,
            PasswordHash = PasswordHash,
            TwoFactorEnabled = false,
            ConcurrencyStamp = ConcurrencyStamp,
            SecurityStamp = SecurityStamp
        };

        return user;
    }


}
