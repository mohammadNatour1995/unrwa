using Domain.Interfaces;

namespace Domain.Entities.Lookup;

public class Logs : IBaseLookup
{
    public Guid? Id { get; set; }
    public string? Message { get; set; } = null;
    public string Level { get; set; }
    public DateTime TimeStamp { get; set; }
    public string? Exception { get; set; } = null;
    public Guid? UserId { get; set; }
    public string? Parameters { get; set; }
    public string? FunctionName { get; set; }
    public string? RequestPath { get; set; }
}
