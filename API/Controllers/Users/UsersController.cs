using Domain.Dtos;
using Domain.Dtos.Users;
using Domain.Dtos.Users.Filters;
using Domain.Interfaces.Users;
using Domain.Utilities.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Users;

public class UsersController(IUserService userService) : SystemBaseController
{
    [HttpPost("Add")]
    public async Task<BaseResponse> Add([FromBody] UserDto dto)
        => await userService.AddAsync(dto);

    [HttpPut("Update")]
    public async Task<BaseResponse> Update([FromBody] UserDto dto)
        => await userService.UpdateAsync(dto);

    [HttpDelete("{id}")]
    public async Task<BaseResponse> Delete(string id)
        => await userService.DeleteAsync(id);

    [HttpGet("All")]
    public async Task<BaseResponse<List<UserDto>>> GetAll()
        => await userService.GetAllAsync();

    [HttpGet("{id}")]
    public async Task<BaseResponse<UserDto>> Find(string id)
        => await userService.FindUserAsync(id);

    [HttpGet("Profile")]
    public async Task<BaseResponse<UserDto>> Profile()
        => await userService.FindUserProfileAsync();

    [HttpPost("Paginated")]
    public async Task<BaseResponse<PaginatedList<UserDto>>> GetPaginated([FromBody] UserFilterDto filter)
        => await userService.GetDataPaginatedAsync(filter);

    [HttpGet("GetRoles")]
    public BaseResponse<List<string>> GetRoles()
        => userService.GetRoles();
}
