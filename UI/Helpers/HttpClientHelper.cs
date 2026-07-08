using AdmiUI.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Domain.Dtos;
using Domain.Dtos.Auth;
using UI.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using static Domain.Enums.ApplicationEnum;

namespace UI.Helpers;

public class HttpClientHelper(
    IHttpContextAccessor _httpContextAccessor,
    IHttpClientFactory _httpClientFactory,
    IOptions<AppSettings> _appSettings,
    IOptions<JwtSettings> _jwtSettings,
    ILogger<HttpClientHelper> _logger)
    : IHttpClientHelper
{
    public async Task<BaseResponse<T>> Send<T>(object req, string path, HttpMethod method, bool? withAuthorization = true)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var accessToken = httpContext != null ? CookieHelper.GetCookie(httpContext.Request, "AccessToken") : null;
        return await ExecuteSendAsync<T>(req, path, method, withAuthorization, accessToken, httpContext, isRetry: false);
    }

    public async Task<BaseResponse> SendCommand(object req, string path, HttpMethod method, bool? withAuthorization = true)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var accessToken = httpContext != null ? CookieHelper.GetCookie(httpContext.Request, "AccessToken") : null;
        return await ExecuteCommandAsync(req, path, method, withAuthorization, accessToken, httpContext, isRetry: false);
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var token = CookieHelper.GetCookie(httpContext?.Request, "AccessToken");

        if (string.IsNullOrEmpty(_jwtSettings.Value.SecretKey))
        {
            RedirectToLogin(httpContext);
            return false;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            if (httpContext != null)
            {
                var refreshed = await TryRefreshTokenAsync(httpContext, null);
                if (refreshed != null)
                    return true;
            }
            RedirectToLogin(httpContext);
            return false;
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            if (!tokenHandler.CanReadToken(token))
            {
                ClearAuthCookies(httpContext);
                RedirectToLogin(httpContext);
                return false;
            }

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.Value.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Value.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Value.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch (SecurityTokenExpiredException)
        {
            if (httpContext != null)
            {
                var newToken = await TryRefreshTokenAsync(httpContext, token);
                if (newToken != null)
                    return true;
            }

            ClearAuthCookies(httpContext);
            RedirectToLogin(httpContext);
            return false;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Invalid security token for path {Path}", httpContext?.Request.Path);
            ClearAuthCookies(httpContext);
            RedirectToLogin(httpContext);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token validation for path {Path}", httpContext?.Request.Path);
            ClearAuthCookies(httpContext);
            RedirectToLogin(httpContext);
            return false;
        }
    }

    private async Task<BaseResponse<T>> ExecuteSendAsync<T>(
        object req, string path, HttpMethod method, bool? withAuthorization,
        string? accessToken, HttpContext? httpContext, bool isRetry)
    {
        var response = new BaseResponse<T>();

        try
        {
            using var client = _httpClientFactory.CreateClient("API");
            using var request = BuildRequest(req, path, method, withAuthorization, accessToken);
            var httpResult = await client.SendAsync(request);

            if (httpResult.IsSuccessStatusCode)
            {
                var json = await httpResult.Content.ReadAsStringAsync();
                var parsed = JsonConvert.DeserializeObject<BaseResponse<object>>(json);
                if (parsed == null) return response;

                if (parsed.Header.Status == ResponseStatus.Success)
                {
                    response.Header = parsed.Header;
                    response.Data = parsed.Data switch
                    {
                        JArray arr => arr.ToObject<T>(),
                        JObject obj => obj.ToObject<T>(),
                        _ when parsed.Data != null => (T)Convert.ChangeType(parsed.Data, typeof(T)),
                        _ => default
                    };
                }
                else
                {
                    response.Header = parsed.Header;
                }
            }
            else if (httpResult.StatusCode == HttpStatusCode.Unauthorized && !isRetry && httpContext != null)
            {
                var newToken = await TryRefreshTokenAsync(httpContext, accessToken);
                if (newToken != null)
                    return await ExecuteSendAsync<T>(req, path, method, withAuthorization, newToken, httpContext, isRetry: true);

                CookieHelper.DeleteAllCookies(httpContext);
                response.Header.Status = ResponseStatus.Unauthorized;
                response.Header.Message = "Session expired. Please sign in again.";
            }
            else
            {
                response.Header.Status = httpResult.StatusCode switch
                {
                    HttpStatusCode.Forbidden => ResponseStatus.Forbidden,
                    _ => ResponseStatus.Error
                };
                response.Header.Message = httpResult.ReasonPhrase ?? httpResult.StatusCode.ToString();
            }
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Request timed out: {Method} {Path}", method, path);
            response.Header.Status = ResponseStatus.Error;
            response.Header.Message = "The request timed out. Please try again.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Send: {Method} {Path}", method, path);
            response.Header.Status = ResponseStatus.Error;
            response.Header.Message = "An unexpected error occurred. Please try again.";
        }

        return response;
    }

    private async Task<BaseResponse> ExecuteCommandAsync(
        object req, string path, HttpMethod method, bool? withAuthorization,
        string? accessToken, HttpContext? httpContext, bool isRetry)
    {
        var response = new BaseResponse();

        try
        {
            using var client = _httpClientFactory.CreateClient("API");
            using var request = BuildRequest(req, path, method, withAuthorization, accessToken);
            var httpResult = await client.SendAsync(request);

            if (httpResult.IsSuccessStatusCode)
            {
                var json = await httpResult.Content.ReadAsStringAsync();
                var parsed = JsonConvert.DeserializeObject<BaseResponse>(json);
                if (parsed != null) response = parsed;
            }
            else if (httpResult.StatusCode == HttpStatusCode.Unauthorized && !isRetry && httpContext != null)
            {
                var newToken = await TryRefreshTokenAsync(httpContext, accessToken);
                if (newToken != null)
                    return await ExecuteCommandAsync(req, path, method, withAuthorization, newToken, httpContext, isRetry: true);

                CookieHelper.DeleteAllCookies(httpContext);
                response.Header.Status = ResponseStatus.Unauthorized;
                response.Header.Message = "Session expired. Please sign in again.";
            }
            else
            {
                response.Header.Status = httpResult.StatusCode switch
                {
                    HttpStatusCode.Forbidden => ResponseStatus.Forbidden,
                    _ => ResponseStatus.Error
                };
                response.Header.Message = httpResult.ReasonPhrase ?? httpResult.StatusCode.ToString();
            }
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Request timed out: {Method} {Path}", method, path);
            response.Header.Status = ResponseStatus.Error;
            response.Header.Message = "The request timed out. Please try again.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during SendCommand: {Method} {Path}", method, path);
            response.Header.Status = ResponseStatus.Error;
            response.Header.Message = "An unexpected error occurred. Please try again.";
        }

        return response;
    }

    private async Task<string?> TryRefreshTokenAsync(HttpContext httpContext, string? currentAccessToken)
    {
        var refreshToken = CookieHelper.GetCookie(httpContext.Request, "RefreshToken");
        if (string.IsNullOrEmpty(refreshToken))
            return null;

        var dto = new RefreshTokenRequestDto
        {
            AccessToken = currentAccessToken ?? string.Empty,
            RefreshToken = refreshToken
        };

        try
        {
            using var client = _httpClientFactory.CreateClient("API");
            using var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/Auth/refresh-token", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<BaseResponse<RefreshTokenResponseDto>>(json);

            if (result?.Header.Status != ResponseStatus.Success || result.Data == null)
                return null;

            CookieHelper.SetCookie(httpContext.Response, "AccessToken", result.Data.AccessToken, result.Data.RefreshTokenExpiration);
            CookieHelper.SetCookie(httpContext.Response, "RefreshToken", result.Data.RefreshToken, result.Data.RefreshTokenExpiration);

            return result.Data.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh token");
            return null;
        }
    }

    private static HttpRequestMessage BuildRequest(object req, string path, HttpMethod method, bool? withAuthorization, string? accessToken)
    {
        var request = new HttpRequestMessage
        {
            Method = method,
            RequestUri = new Uri(path, UriKind.Relative),
            Content = new StringContent(JsonConvert.SerializeObject(req), Encoding.UTF8, "application/json"),
        };

        if (withAuthorization == true && !string.IsNullOrEmpty(accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return request;
    }

    private static void RedirectToLogin(HttpContext? httpContext)
    {
        if (httpContext is null) return;
        var path = httpContext.Request.Path.Value ?? string.Empty;
        if (!path.Equals("/Account/Signin", StringComparison.OrdinalIgnoreCase))
        {
            var returnUrl = Uri.EscapeDataString(httpContext.Request.Path + httpContext.Request.QueryString);
            httpContext.Response.Redirect($"/Account/Signin?returnUrl={returnUrl}");
        }
    }

    private static void ClearAuthCookies(HttpContext? httpContext)
    {
        if (httpContext == null) return;
        httpContext.Response.Cookies.Delete("AccessToken");
        httpContext.Response.Cookies.Delete("RefreshToken");
    }
}
