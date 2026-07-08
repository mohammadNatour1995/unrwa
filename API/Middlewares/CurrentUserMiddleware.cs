using Domain.Dtos.Users;
using Domain.Interfaces.Users;
using System.Security.Claims;

namespace Api.Middlewares;

public class CurrentUserMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext, ICurrentUser currentUser)
    {
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var identity = (ClaimsIdentity)httpContext.User.Identity;
            currentUser.Info = new ApplicationUserDto
            {
                Id = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
                UserName = identity.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
                Email = identity.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
                FullName = identity.FindFirst("FullName")?.Value ?? string.Empty,
                IsActive = true
            };
        }
        await _next.Invoke(httpContext);
    }
}

public static class CurrentUserMiddlewareExtensions
{
    public static IApplicationBuilder UseCurrentUserMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CurrentUserMiddleware>();
    }
}
