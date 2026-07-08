using Application.Interfaces.Logging;
using Domain.Dtos;
using Domain.Dtos.Auth;
using Domain.Entities.Users;
using Domain.Interfaces.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using static Domain.Enums.ApplicationEnum;

namespace Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly ILoggerManager<AuthService> _loggerManager;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService,
            ILoggerManager<AuthService> loggerManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _loggerManager = loggerManager;
        }

        public async Task<BaseResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            var response = new BaseResponse<LoginResponseDto>();

            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    response.Header.Status = ResponseStatus.Unauthorized;
                    response.Header.Message = "Invalid email or password.";
                    return response;
                }

                if (!user.IsActive)
                {
                    response.Header.Status = ResponseStatus.Forbidden;
                    response.Header.Message = "Account is deactivated.";
                    return response;
                }

                var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!signInResult.Succeeded)
                {
                    response.Header.Status = ResponseStatus.Unauthorized;
                    response.Header.Message = "Invalid email or password.";
                    return response;
                }

                var tokens = await _jwtService.GenerateTokensAsync(user);

                response.Data = tokens;
                response.Header.Status = ResponseStatus.Success;
                response.Header.Message = "Login successful.";
            }
            catch (Exception ex)
            {
                _loggerManager.Error(ex);
                response.Header.Status = ResponseStatus.Error;
                response.Header.Message = "An error occurred during login.";
            }

            return response;
        }

        public async Task<BaseResponse<RefreshTokenResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var response = new BaseResponse<RefreshTokenResponseDto>();

            try
            {
                var tokens = await _jwtService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);

                response.Data = tokens;
                response.Header.Status = ResponseStatus.Success;
                response.Header.Message = "Token refreshed successfully.";
            }
            catch (SecurityTokenException ex)
            {
                _loggerManager.Error(ex);
                response.Header.Status = ResponseStatus.Unauthorized;
                response.Header.Message = "Invalid or expired token.";
            }
            catch (Exception ex)
            {
                _loggerManager.Error(ex);
                response.Header.Status = ResponseStatus.Error;
                response.Header.Message = "An error occurred while refreshing token.";
            }

            return response;
        }

        public async Task<BaseResponse> LogoutAsync(string refreshToken)
        {
            var response = new BaseResponse();

            try
            {
                var revoked = await _jwtService.RevokeRefreshTokenAsync(refreshToken);
                if (revoked)
                {
                    response.Header.Status = ResponseStatus.Success;
                    response.Header.Message = "Logout successful.";
                }
                else
                {
                    response.Header.Status = ResponseStatus.NotFound;
                    response.Header.Message = "Token not found.";
                }
            }
            catch (Exception ex)
            {
                _loggerManager.Error(ex);
                response.Header.Status = ResponseStatus.Error;
                response.Header.Message = "An error occurred during logout.";
            }

            return response;
        }
    }
}
