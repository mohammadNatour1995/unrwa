using Domain.Dtos.Lookup;
using Domain.Entities.Lookup;
using Domain.Utilities.Common;

namespace Domain.Interfaces.Lookup;

public interface IExceptionsRepository : IBaseLookupRepository<Logs>
{
    Task<PaginatedList<Logs>> GetDataPaginated(LogsFilterDto dto, CancellationToken cancellationToken = default);
    Task<LogsStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default);
}
