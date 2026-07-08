using Domain.Dtos.Lookup;
using Domain.Entities.Lookup;
using Domain.Interfaces.Lookup;
using Domain.Utilities.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Lookup;

public class ExceptionsRepository : BaseLookupRepository<Logs>, IExceptionsRepository
{
    public ExceptionsRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
    {
    }

    public async Task<PaginatedList<Logs>> GetDataPaginated(LogsFilterDto dto, CancellationToken cancellationToken = default)
    {
        var query = _context.Logs
            .AsNoTracking();

        if (!string.IsNullOrEmpty(dto.Message))
            query = query.Where(x => x.Message != null && x.Message.Contains(dto.Message));

        if (!string.IsNullOrEmpty(dto.Level))
            query = query.Where(x => x.Level.Contains(dto.Level));

        if (!string.IsNullOrEmpty(dto.FunctionName))
            query = query.Where(x => x.FunctionName != null && x.FunctionName.Contains(dto.FunctionName));

        if (dto.TimeStampFrom.HasValue)
            query = query.Where(x => x.TimeStamp >= dto.TimeStampFrom.Value);

        if (dto.TimeStampTo.HasValue)
            query = query.Where(x => x.TimeStamp <= dto.TimeStampTo.Value);

        var totalCount = await query.CountAsync();

        var paginatedData = await query
            .OrderByDescending(x => x.TimeStamp)
            .Skip((dto.PageNumber - 1) * dto.PageSize)
            .Take(dto.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<Logs>(
            paginatedData,
            totalCount,
            dto.PageNumber,
            dto.PageSize
        );
    }

    public async Task<LogsStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var query = _context.Logs.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);
        var errorCount = await query.CountAsync(x => x.Level == "Error", cancellationToken);
        var warningCount = await query.CountAsync(x => x.Level == "Warning", cancellationToken);
        var informationCount = await query.CountAsync(x => x.Level == "Information", cancellationToken);

        var fromDate = DateTime.UtcNow.Date.AddDays(-6);
        var dailyCounts = await query
            .Where(x => x.TimeStamp >= fromDate)
            .GroupBy(x => x.TimeStamp.Date)
            .Select(g => new LogsDailyCountDto { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        return new LogsStatisticsDto
        {
            TotalCount = totalCount,
            ErrorCount = errorCount,
            WarningCount = warningCount,
            InformationCount = informationCount,
            DailyCounts = dailyCounts
        };
    }
}
