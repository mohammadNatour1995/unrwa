using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Logging;
using Domain.Constants;
using Domain.Dtos;
using Domain.Dtos.Users;
using Domain.Dtos.Users.Filters;
using Domain.Entities.Users;
using Domain.Interfaces;
using Domain.Interfaces.Users;
using Domain.Utilities.Common;
using static Domain.Enums.ApplicationEnum;

namespace Application.Services.Users;

public class UserService : IUserService
{
    private readonly ApplicationUserManager _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ILoggerManager<UserService> _loggerManager;

    public UserService(
        ApplicationUserManager userManager,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ILoggerManager<UserService> loggerManager)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _loggerManager = loggerManager;
    }

    public async Task<BaseResponse> AddAsync(UserDto dto)
    {
        var result = new BaseResponse();
        try
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (await IsEmailUsedAsync(dto.Email.Trim()))
                {
                    Fail(result, "Email already used.");
                    return;
                }

                if (await IsUserNameUsedAsync(dto.UserName.Trim()))
                {
                    Fail(result, "Username already used.");
                    return;
                }

                var user = new ApplicationUser
                {
                    Email = dto.Email,
                    FullName = dto.FullName,
                    UserName = dto.UserName,
                    PhoneNumber = dto.PhoneNumber,
                    IsActive = true,
                    LockoutEnabled = false
                };

                var createResult = await _userManager.CreateAsync(user, dto.Password!);
                if (!createResult.Succeeded)
                {
                    _loggerManager.Warning("Error adding user.", createResult.Errors.ToList());
                    Fail(result, "An error occurred while creating the user.");
                    return;
                }

                await _userManager.AddToRoleAsync(user, dto.Role);
                result.Header = new BaseHeader { Status = ResponseStatus.Success, Message = "User added successfully." };
            });
        }
        catch (Exception ex)
        {
            _loggerManager.Error(ex, dto);
            return Fail(result, "An error occurred while adding the user.");
        }
        return result;
    }

    public async Task<BaseResponse> UpdateAsync(UserDto dto)
    {
        var result = new BaseResponse();
        try
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (await IsEmailUsedAsync(dto.Email.Trim(), dto.Id))
                {
                    Fail(result, "Email already used.");
                    return;
                }

                if (await IsUserNameUsedAsync(dto.UserName.Trim(), dto.Id))
                {
                    Fail(result, "Username already used.");
                    return;
                }

                var user = await _userManager.FindByIdAsync(dto.Id!);
                if (user == null)
                {
                    result.Header = new BaseHeader { Status = ResponseStatus.NotFound, Message = "User not found." };
                    return;
                }

                var currentRole = await GetRoleByUserIdAsync(dto.Id!);
                if (!string.IsNullOrEmpty(currentRole))
                    await _userManager.RemoveFromRoleAsync(user, currentRole);

                user.Email = dto.Email;
                user.FullName = dto.FullName;
                user.UserName = dto.UserName;
                user.PhoneNumber = dto.PhoneNumber;
                user.IsActive = dto.IsActive;

                await _userManager.UpdateAsync(user);
                await _userManager.AddToRoleAsync(user, dto.Role);

                result.Header = new BaseHeader { Status = ResponseStatus.Success, Message = "User updated successfully." };
            });
        }
        catch (Exception ex)
        {
            _loggerManager.Error(ex, dto);
            return Fail(result, "An error occurred while updating the user.");
        }
        return result;
    }

    public async Task<BaseResponse> DeleteAsync(string id)
    {
        var result = new BaseResponse();
        try
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    result.Header = new BaseHeader { Status = ResponseStatus.NotFound, Message = "User not found." };
                    return;
                }

                var deleteResult = await _userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    _loggerManager.Warning("Error deleting user.", deleteResult.Errors.ToList());
                    Fail(result, "An error occurred while deleting the user.");
                    return;
                }

                result.Header = new BaseHeader { Status = ResponseStatus.Success, Message = "User deleted successfully." };
            });
        }
        catch (Exception ex)
        {
            _loggerManager.Error(ex, new { userId = id });
            return Fail(result, "An error occurred while deleting the user.");
        }
        return result;
    }

    public async Task<BaseResponse<PaginatedList<UserDto>>> GetDataPaginatedAsync(UserFilterDto filter)
    {
        var result = new BaseResponse<PaginatedList<UserDto>>();
        try
        {
            var adminIds = await _userManager.GetUserIdsInRoleAsync(RoleSet.Administrator.NormalizedName);

            var query = _userManager.Users.AsNoTracking()
                .Where(x => !adminIds.Contains(x.Id))
                .Select(x => new UserDto
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    FullName = x.FullName,
                    IsActive = x.IsActive
                });

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(f => f.Id)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var roleMap = await _userManager.GetUserRoleMapAsync(items.Select(x => x.Id!));
            foreach (var item in items)
                item.Role = roleMap.GetValueOrDefault(item.Id!, string.Empty);

            result.Data = new PaginatedList<UserDto>(items, totalCount, filter.PageNumber, filter.PageSize);
            result.Header = new BaseHeader { Status = ResponseStatus.Success, Message = "Users retrieved successfully." };
        }
        catch (Exception ex)
        {
            _loggerManager.Error(ex, filter);
            return Fail<PaginatedList<UserDto>>(result, "An error occurred while retrieving users.");
        }
        return result;
    }

    public async Task<BaseResponse<List<UserDto>>> GetAllAsync()
    {
        var result = new BaseResponse<List<UserDto>>();
        try
        {
            var adminIds = await _userManager.GetUserIdsInRoleAsync(RoleSet.Administrator.NormalizedName);

            var users = await _userManager.Users.AsNoTracking()
                .Where(x => !adminIds.Contains(x.Id))
                .Select(x => new UserDto
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    FullName = x.FullName,
                    IsActive = x.IsActive
                }).ToListAsync();

            var roleMap = await _userManager.GetUserRoleMapAsync(users.Select(x => x.Id!));
            foreach (var item in users)
                item.Role = roleMap.GetValueOrDefault(item.Id!, string.Empty);

            result.Data = users;
            result.Header = new BaseHeader { Status = ResponseStatus.Success, Message = "Users retrieved successfully." };
        }
        catch (Exception ex)
        {
            _loggerManager.Error(ex, null);
            return Fail<List<UserDto>>(result, "An error occurred while retrieving users.");
        }
        return result;
    }

    public async Task<BaseResponse<UserDto>> FindUserProfileAsync()
    {
        var result = new BaseResponse<UserDto>();
        try
        {
            var user = await _userManager.Users.AsNoTracking()
                .Where(x => x.Id == _currentUser.Info.Id)
                .Select(x => new UserDto
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    FullName = x.FullName,
                    IsActive = x.IsActive
                }).FirstOrDefaultAsync();

            if (user is null)
            {
                result.Header = new BaseHeader { Status = ResponseStatus.NotFound, Message = "User not found." };
                return result;
            }

            user.Role = await GetRoleByUserIdAsync(user.Id!);
            result.Data = user;
            result.Header = new BaseHeader { Status = ResponseStatus.Success, Message = "User profile retrieved successfully." };
        }
        catch (Exception ex)
        {
            _loggerManager.Error(ex, null);
            return Fail<UserDto>(result, "An error occurred while retrieving the user profile.");
        }
        return result;
    }

    public async Task<BaseResponse<UserDto>> FindUserAsync(string userId)
    {
        var result = new BaseResponse<UserDto>();
        try
        {
            var user = await _userManager.Users.AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => new UserDto
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    FullName = x.FullName,
                    IsActive = x.IsActive
                }).FirstOrDefaultAsync();

            if (user is null)
            {
                result.Header = new BaseHeader { Status = ResponseStatus.NotFound, Message = "User not found." };
                return result;
            }

            user.Role = await GetRoleByUserIdAsync(user.Id!);
            result.Data = user;
            result.Header = new BaseHeader { Status = ResponseStatus.Success, Message = "User retrieved successfully." };
        }
        catch (Exception ex)
        {
            _loggerManager.Error(ex, new { userId =userId });
            return Fail<UserDto>(result, "An error occurred while retrieving the user.");
        }
        return result;
    }

    public BaseResponse<List<string>> GetRoles() =>
        new()
        {
            Data = RoleSet.GetAllRolesName(),
            Header = new BaseHeader { Status = ResponseStatus.Success, Message = "Roles retrieved successfully." }
        };

    #region Helpers

    private async Task<string> GetRoleByUserIdAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return string.Empty;
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return string.Empty;
        var roles = await _userManager.GetRolesAsync(user);
        return roles.FirstOrDefault() ?? string.Empty;
    }

    private async Task<bool> IsEmailUsedAsync(string email, string? excludeUserId = null)
    {
        var query = _userManager.Users.Where(x => x.Email != null && x.Email.ToLower() == email.ToLower());
        if (!string.IsNullOrEmpty(excludeUserId))
            query = query.Where(x => x.Id != excludeUserId);
        return await query.AnyAsync();
    }

    private async Task<bool> IsUserNameUsedAsync(string userName, string? excludeUserId = null)
    {
        var query = _userManager.Users.Where(x => x.UserName != null && x.UserName.ToLower() == userName.ToLower());
        if (!string.IsNullOrEmpty(excludeUserId))
            query = query.Where(x => x.Id != excludeUserId);
        return await query.AnyAsync();
    }

    private static BaseResponse Fail(BaseResponse result, string message)
    {
        result.Header = new BaseHeader { Status = ResponseStatus.Error, Message = message };
        return result;
    }

    private static BaseResponse<T> Fail<T>(BaseResponse<T> result, string message)
    {
        result.Header = new BaseHeader { Status = ResponseStatus.Error, Message = message };
        return result;
    }

    #endregion
}
