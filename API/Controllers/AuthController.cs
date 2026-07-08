using Domain.Dtos;
using Domain.Dtos.Auth;
using Domain.Interfaces.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<BaseResponse<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        return await _authService.LoginAsync(request);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<BaseResponse<RefreshTokenResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        return await _authService.RefreshTokenAsync(request);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("logout")]
    public async Task<BaseResponse> Logout([FromBody] string refreshToken)
    {
        return await _authService.LogoutAsync(refreshToken);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var user = new
        {
            Id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            UserName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
            Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
            FullName = User.FindFirst("FullName")?.Value,
            Roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToArray()
        };

        return Ok(new { Data = user, Header = new { Status = Domain.Enums.ApplicationEnum.ResponseStatus.Success } });
    }
}
