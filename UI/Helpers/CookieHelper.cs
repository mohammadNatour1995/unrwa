using Domain.Helpers;

namespace UI.Helpers;

public static class CookieHelper
{
    public static void SetCookie(HttpResponse response, string key, string value, DateTime? expiresAt = null)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt ?? DateTime.UtcNow.AddDays(7)
        };
        response.Cookies.Append(key, DomainHelpers.Encrypt(value), options);
    }

    public static string GetCookie(HttpRequest? request, string key)
    {
        if (request == null) return string.Empty;
        try
        {
            return DomainHelpers.Decrypt(request.Cookies[key]);
        }
        catch
        {
            return string.Empty;
        }
    }

    public static void DeleteCookie(HttpResponse response, string key)
        => response.Cookies.Delete(key);

    public static void DeleteAllCookies(HttpContext context)
    {
        foreach (var key in context.Request.Cookies.Keys.ToList())
            context.Response.Cookies.Delete(key);
    }
}
