using Domain.Dtos;
using Domain.Dtos.Lookup;
using Domain.Entities.Lookup;
using Domain.Utilities.Common;

namespace Application.Interfaces.Lookup;

public interface IExceptionsManager
{
    Task<BaseResponse<LogDto>> FindAsync(Guid id);

    Task<BaseResponse<PaginatedList<Logs>>> GetDataPaginated(LogsFilterDto dto);

    Task<BaseResponse<LogsStatisticsDto>> GetStatisticsAsync();
}
