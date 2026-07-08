using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Application.Interfaces.Logging;
using Domain.Interfaces.Users;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System.Runtime.CompilerServices;

namespace Application.Services.Logging;

public class LoggerManager<T> : ILoggerManager<T>
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly ICurrentUser _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoggerManager(IConfiguration configuration, ICurrentUser currentUser, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
        _logger = Log.ForContext<T>();
    }

    public void Fatal(Exception exception, string message = "An error occurred")
    {
        _logger.Fatal(exception, message);
    }

    public void Error(
        Exception exception,
        string message = null,
        object? parameters = null,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int line = 0
        )
    {
        WriteLog(LogEventLevel.Error, exception, message, parameters, caller);
    }

    public void Error<TProp>(
        Exception exception,
        TProp propertyValue,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int line = 0)
    {
        string username = _currentUser.Info?.UserName ?? "anonymous";
        Guid? userId = Guid.TryParse(_currentUser.Info?.Id, out var parsed) ? parsed : (Guid?)null;
        string? requestPath = _httpContextAccessor.HttpContext?.Request?.Path.Value;

        _logger
            .ForContext("Id", Guid.NewGuid())
            .ForContext("UserName", username)
            .ForContext("UserId", userId)
            .ForContext("RequestPath", requestPath)
            .ForContext("Parameters", JsonConvert.SerializeObject(propertyValue))
            .ForContext("FilePath", $"{filePath}:{line}")
            .ForContext("FunctionName", caller)
            .Error(exception, exception.Message);
    }

    public void Warning(string message, object obj)
    {
        _logger.Warning(message, obj);
    }

    public void Debug(string message, object obj)
    {
        bool.TryParse(_configuration["Logging:SeriLog:DebugEnabled"], out bool debugEnabled);

        if (debugEnabled)
            _logger.Debug(message, obj);
    }
    private void WriteLog(
    LogEventLevel level,
    Exception exception,
    string message,
    object? parameters,
    string caller)
    {
        string username = _currentUser.Info?.UserName ?? "anonymous";
        Guid? userId = Guid.TryParse(_currentUser.Info?.Id, out var parsed) ? parsed : (Guid?)null;
        string? requestPath = _httpContextAccessor.HttpContext?.Request?.Path.Value;

        _logger
            .ForContext("Id", Guid.NewGuid())
            .ForContext("UserName", username)
            .ForContext("UserId", userId)
            .ForContext("RequestPath", requestPath)
            .ForContext("FunctionName", caller)
            .ForContext("StackTrace", exception.StackTrace)
            .ForContext(
                "Parameters",
                parameters != null
                    ? JsonConvert.SerializeObject(parameters)
                    : null)
            .Write(level, exception, message);
    }
}
