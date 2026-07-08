namespace Domain.Enums;

public partial class ApplicationEnum
{
    public enum ResponseStatus
    {
        Success = 1,
        Error,
        NotFound,
        Unauthorized,
        BadRequest,
        Forbidden,
        Conflict,
        Validation,
        InternalServerError
    }
}
