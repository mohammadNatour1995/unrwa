
using Domain.Records;

namespace Domain.Constants;

public static class RoleSet
{
    public static readonly RoleDefinition Administrator = new("5F82699A-59FB-40FC-8D8A-DA14E195B209", "Administrator");
    public static readonly RoleDefinition Admin = new("F49BD174-9FD6-41FA-966E-BB41548F2E80", "Admin");
  
    public static List<string> GetAllRolesName() => new()
    {
        Admin.Name,
    };

    public static string All = $"{Administrator.Name},{Admin.Name}";
}
