namespace Domain.Dtos.Lookup;

public class LogsStatisticsDto
{
    public int TotalCount { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int InformationCount { get; set; }
    public List<LogsDailyCountDto> DailyCounts { get; set; } = new();
}

public class LogsDailyCountDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}