using Domain.Dtos;
using Domain.Dtos.Users;
using Domain.Dtos.Users.Filters;
using Domain.Utilities.Common;

namespace Domain.Interfaces.Users;

public interface IUserService
{
    Task<BaseResponse> AddAsync(UserDto dto);
    Task<BaseResponse> UpdateAsync(UserDto dto);
    Task<BaseResponse> DeleteAsync(string id);
    Task<BaseResponse<PaginatedList<UserDto>>> GetDataPaginatedAsync(UserFilterDto filter);
    Task<BaseResponse<List<UserDto>>> GetAllAsync();
    Task<BaseResponse<UserDto>> FindUserProfileAsync();
    Task<BaseResponse<UserDto>> FindUserAsync(string userId);
    BaseResponse<List<string>> GetRoles();
}
