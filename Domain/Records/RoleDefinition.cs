using Domain.Entities.Users;

namespace Domain.Records;

public record RoleDefinition(string Id, string Name)
{
    public string NormalizedName => Name.ToUpperInvariant();

    public ApplicationRole ToEntity() => new()
    {
        Id = Id,
        Name = Name,
        NormalizedName = NormalizedName
    };

}
