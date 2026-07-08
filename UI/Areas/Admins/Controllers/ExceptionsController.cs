using AdmiUI.Helpers;
using Microsoft.AspNetCore.Mvc;
using Domain.Dtos;
using Domain.Dtos.Lookup;
using Domain.Entities.Lookup;
using Domain.Utilities.Common;
using UI.Helpers;

namespace UI.Areas.Admins.Controllers;

[Area("Admins")]
[Route("Admins/[controller]/[action]")]
public class ExceptionsController(
    IHttpClientHelper httpClientHelper,
    IHttpContextAccessor httpContextAccessor)
    : BaseWebController(httpClientHelper, httpContextAccessor)
{
    public IActionResult Manage() => View();
    public IActionResult Details() => View();

    [HttpPost]
    public async Task<BaseResponse<LogDto>> Find(Guid id)
        => await _HttpClientHelper.Send<LogDto>(id, "api/Exceptions/" + id, HttpMethod.Get);

    [HttpPost]
    public async Task<BaseResponse<PaginatedList<Logs>>> ReadAllPagination(LogsFilterDto logsDto)
        => await _HttpClientHelper.Send<PaginatedList<Logs>>(logsDto, "api/Exceptions/Paginated", HttpMethod.Post);
}
