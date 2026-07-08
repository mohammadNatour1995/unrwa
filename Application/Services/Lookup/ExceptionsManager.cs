using Microsoft.AspNetCore.Identity;
using Application.Interfaces.Logging;
using Application.Interfaces.Lookup;
using Domain.Dtos;
using Domain.Dtos.Lookup;
using Domain.Entities.Lookup;
using Domain.Entities.Users;
using Domain.Interfaces.Lookup;
using Domain.Utilities.Common;
using static Domain.Enums.ApplicationEnum;

namespace Application.Services.Lookup;

public class ExceptionsManager(
        IExceptionsRepository exceptionsRepository,
        ILoggerManager<ExceptionsManager> loggerManager,
        UserManager<ApplicationUser> userManager) : IExceptionsManager
{
    public async Task<BaseResponse<LogDto>> FindAsync(Guid id)
    {
        var result = new BaseResponse<LogDto>();
        try
        {
            var data = await exceptionsRepository.FindAsync(id);
            if (data is null)
            {
                result.Header = new BaseHeader { Status = ResponseStatus.NotFound, Message = "Log not found." };
                return result;
            }

            string? userName = null;
            if (data.UserId.HasValue)
            {
                var user = await userManager.FindByIdAsync(data.UserId.Value.ToString());
                userName = user?.UserName;
            }

            result.Data = new LogDto
            {
                Id = data.Id,
                Message = data.Message,
                Level = data.Level,
                TimeStamp = data.TimeStamp,
                Exception = data.Exception,
                FunctionName = data.FunctionName,
                RequestPath = data.RequestPath,
                Parameters = data.Parameters,
                UserName = userName
            };
            result.Header = new BaseHeader { Status = ResponseStatus.Success, Message = "Log retrieved successfully." };
        }
        catch (Exception ex)
        {
            result.Header = new BaseHeader { Status = ResponseStatus.Error, Message = "An error occurred while retrieving the log." };
            loggerManager.Error(ex, id);
        }
        return result;
    }

    public async Task<BaseResponse<PaginatedList<Logs>>> GetDataPaginated(LogsFilterDto dto)
    {
        var result = new BaseResponse<PaginatedList<Logs>>();
        try
        {
            result.Data = await exceptionsRepository.GetDataPaginated(dto);
            result.Header = new BaseHeader { Status = ResponseStatus.Success, Message = "Logs retrieved successfully." };
        }
        catch (Exception ex)
        {
            result.Header = new BaseHeader { Status = ResponseStatus.Error, Message = "An error occurred while retrieving logs." };
            loggerManager.Error(ex, dto);
        }
        return result;
    }

    public async Task<BaseResponse<LogsStatisticsDto>> GetStatisticsAsync()
    {
        var result = new BaseResponse<LogsStatisticsDto>();
        try
        {
            result.Data = await exceptionsRepository.GetStatisticsAsync();
            result.Header = new BaseHeader { Status = ResponseStatus.Success, Message = "Logs statistics retrieved successfully." };
        }
        catch (Exception ex)
        {
            result.Header = new BaseHeader { Status = ResponseStatus.Error, Message = "An error occurred while retrieving logs statistics." };
            loggerManager.Error(ex, null);
        }
        return result;
    }
}
