using AdmiUI.Helpers;
using Microsoft.AspNetCore.Mvc;
using Domain.Dtos;
using Domain.Dtos.Auth;
using UI.Helpers;
using static Domain.Enums.ApplicationEnum;

namespace UI.Controllers;

public class AccountController : BaseWebController
{
    public AccountController(IHttpClientHelper httpClientHelper, IHttpContextAccessor httpContextAccessor)
        : base(httpClientHelper, httpContextAccessor)
    {
    }

    public IActionResult Signin([FromQuery] string? returnUrl = null)
    {
        var accessToken = CookieHelper.GetCookie(_HttpContextAccessor.HttpContext?.Request, "AccessToken");
        if (string.IsNullOrEmpty(accessToken))
        {
            CookieHelper.DeleteAllCookies(_HttpContextAccessor.HttpContext!);
            return View();
        }
        if (!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/") && !returnUrl.StartsWith("//"))
            return Redirect(returnUrl);
        return RedirectToAction("Dashboard", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        try
        {
            var ctx = _HttpContextAccessor.HttpContext!;
            var refreshToken = CookieHelper.GetCookie(ctx.Request, "RefreshToken");
            if (!string.IsNullOrEmpty(refreshToken))
                await _HttpClientHelper.SendCommand(refreshToken, "api/Auth/logout", HttpMethod.Post);

            CookieHelper.DeleteAllCookies(ctx);
        }
        catch
        {
            // always redirect to sign-in even if logout call fails
        }
        return RedirectToAction("Signin", "Account");
    }

    public IActionResult Settings() => View();

    public async Task<BaseResponse<LoginResponseDto>> Login([FromBody] LoginRequestDto user)
    {
        var baseResponse = new BaseResponse<LoginResponseDto>();
        try
        {
            baseResponse = await _HttpClientHelper.Send<LoginResponseDto>(user, "api/Auth/login", HttpMethod.Post, false);
            if (baseResponse.Header.Status == ResponseStatus.Success && baseResponse.Data != null)
            {
                var ctx = _HttpContextAccessor.HttpContext!;
                CookieHelper.SetCookie(ctx.Response, "AccessToken", baseResponse.Data.AccessToken, baseResponse.Data.RefreshTokenExpiration);
                CookieHelper.SetCookie(ctx.Response, "RefreshToken", baseResponse.Data.RefreshToken, baseResponse.Data.RefreshTokenExpiration);
                CookieHelper.SetCookie(ctx.Response, "Name", baseResponse.Data.User.UserName);
                CookieHelper.SetCookie(ctx.Response, "UserRoles", string.Join(",", baseResponse.Data.User.Roles), baseResponse.Data.RefreshTokenExpiration);
            }
        }
        catch (Exception ex)
        {
            baseResponse.Header.Status = ResponseStatus.Error;
            baseResponse.Header.Message = ex.Message;
        }
        return baseResponse;
    }
}
