namespace Domain.Dtos.Lookup;

public class LogsFilterDto : BaseFilterDto
{
    public string? Message { get; set; }
    public string? Level { get; set; }
    public string? FunctionName { get; set; }
    public DateTime? TimeStampFrom { get; set; }
    public DateTime? TimeStampTo { get; set; }
}
