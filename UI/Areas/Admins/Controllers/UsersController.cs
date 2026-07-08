using AdmiUI.Helpers;
using Microsoft.AspNetCore.Mvc;
using Domain.Dtos;
using Domain.Dtos.Users;
using Domain.Dtos.Users.Filters;
using Domain.Utilities.Common;
using UI.Helpers;

namespace UI.Areas.Admins.Controllers;

[Area("Admins")]
[Route("Admins/[controller]/[action]")]
public class UsersController(
    IHttpClientHelper httpClientHelper,
    IHttpContextAccessor httpContextAccessor)
    : BaseWebController(httpClientHelper, httpContextAccessor)
{
    public IActionResult Manage() => View();

    [HttpPost]
    public async Task<BaseResponse<PaginatedList<UserDto>>> ReadAllPagination(UserFilterDto filter)
        => await _HttpClientHelper.Send<PaginatedList<UserDto>>(filter, "api/Users/Paginated", HttpMethod.Post);

    [HttpPost]
    public async Task<BaseResponse<UserDto>> Find(string id)
        => await _HttpClientHelper.Send<UserDto>(id, "api/Users/" + id, HttpMethod.Get);

    [HttpPost]
    public async Task<BaseResponse> Add(UserDto dto)
        => await _HttpClientHelper.SendCommand(dto, "api/Users/Add", HttpMethod.Post);

    [HttpPost]
    public async Task<BaseResponse> Update(UserDto dto)
        => await _HttpClientHelper.SendCommand(dto, "api/Users/Update", HttpMethod.Put);

    [HttpPost]
    public async Task<BaseResponse> Delete(string id)
        => await _HttpClientHelper.SendCommand(id, "api/Users/" + id, HttpMethod.Delete);

    [HttpPost]
    public async Task<BaseResponse<List<string>>> GetRoles()
        => await _HttpClientHelper.Send<List<string>>(new { }, "api/Users/GetRoles", HttpMethod.Get);
}
