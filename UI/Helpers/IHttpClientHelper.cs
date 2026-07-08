using Domain.Dtos;

namespace AdmiUI.Helpers;

public interface IHttpClientHelper
{
    Task<BaseResponse<T>> Send<T>(object req, string path, HttpMethod method, bool? withAuthorization = true);
    Task<BaseResponse> SendCommand(object req, string path, HttpMethod method, bool? withAuthorization = true);
    Task<bool> IsAuthenticatedAsync();
}
