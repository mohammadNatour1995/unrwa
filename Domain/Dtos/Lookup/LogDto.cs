namespace Domain.Dtos.Lookup;

public class LogDto
{
    public Guid? Id { get; set; }
    public string? Message { get; set; }
    public string Level { get; set; } = string.Empty;
    public DateTime TimeStamp { get; set; }
    public string? Exception { get; set; }
    public string? UserName { get; set; }
    public string? Parameters { get; set; }
    public string? FunctionName { get; set; }
    public string? RequestPath { get; set; }
}
