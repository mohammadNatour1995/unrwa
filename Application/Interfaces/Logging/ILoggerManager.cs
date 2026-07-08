using System.Runtime.CompilerServices;

namespace Application.Interfaces.Logging
{
    public interface ILoggerManager<T> 
    {
        void Fatal(Exception exception, string message = "An error occurred");

        void Error(
             Exception exception,
        string message = null,
        object? parameters = null,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int line = 0
             );
        void Error<TProp>(
                Exception exception,
        TProp propertyValue,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int line = 0
             );

        void Warning(string message, object obj);

        void Debug(string message, object obj);
    }
}
