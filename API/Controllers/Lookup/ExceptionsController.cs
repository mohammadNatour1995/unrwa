using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Lookup;
using Domain.Dtos;
using Domain.Dtos.Lookup;
using Domain.Entities.Lookup;
using Domain.Utilities.Common;

namespace Api.Controllers.Lookup;

public class ExceptionsController(IExceptionsManager exceptionsManager) : SystemBaseController
{
    [HttpGet("{id:guid}")]
    public async Task<BaseResponse<LogDto>> Find(Guid id)
        => await exceptionsManager.FindAsync(id);

    [HttpPost("Paginated")]
    public async Task<BaseResponse<PaginatedList<Logs>>> GetPaginated([FromBody] LogsFilterDto filter)
        => await exceptionsManager.GetDataPaginated(filter);

    [HttpGet("Statistics")]
    public async Task<BaseResponse<LogsStatisticsDto>> GetStatistics()
        => await exceptionsManager.GetStatisticsAsync();
}
