using static Domain.Enums.ApplicationEnum;

namespace Domain.Dtos;

public class BaseResponse<T>
{
    public BaseHeader Header { get; set; } = new BaseHeader();

    public T Data { get; set; } 
}

public class BaseResponse
{
    public BaseHeader Header { get; set; } = new BaseHeader();
}

public class BaseHeader
{
    public ResponseStatus Status { get; set; }

    public string Message { get; set; }
}
