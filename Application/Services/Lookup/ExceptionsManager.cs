using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Logging;
using Application.Interfaces.Lookup;
using Domain.Dtos;
using Domain.Dtos.Lookup;
using Domain.Entities.Lookup;
using Domain.Entities.Users;
using Domain.Interfaces;
using Domain.Utilities.Common;
using static Domain.Enums.ApplicationEnum;

namespace Application.Services.Lookup;

public class ExceptionsManager(
        IEFBaseLookupRepository<Logs> exceptionsRepository,
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
            var query = exceptionsRepository.Query();

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
                .ToListAsync();

            result.Data = new PaginatedList<Logs>(paginatedData, totalCount, dto.PageNumber, dto.PageSize);
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
            var query = exceptionsRepository.Query();

            var totalCount = await query.CountAsync();
            var errorCount = await query.CountAsync(x => x.Level == "Error");
            var warningCount = await query.CountAsync(x => x.Level == "Warning");
            var informationCount = await query.CountAsync(x => x.Level == "Information");

            var fromDate = DateTime.UtcNow.Date.AddDays(-6);
            var dailyCounts = await query
                .Where(x => x.TimeStamp >= fromDate)
                .GroupBy(x => x.TimeStamp.Date)
                .Select(g => new LogsDailyCountDto { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            result.Data = new LogsStatisticsDto
            {
                TotalCount = totalCount,
                ErrorCount = errorCount,
                WarningCount = warningCount,
                InformationCount = informationCount,
                DailyCounts = dailyCounts
            };
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
