using Application.Interfaces.Logging;
using Domain.Dtos.Auth;
using Domain.Dtos.Users;
using Domain.Entities.Auth;
using Domain.Entities.Users;
using Domain.Interfaces;
using Domain.Interfaces.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services.Auth
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEFBaseRepository<RefreshToken> _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerManager<JwtService> _loggerManager;

        public JwtService(
            IOptions<JwtSettings> jwtSettings,
            UserManager<ApplicationUser> userManager,
            IEFBaseRepository<RefreshToken> refreshTokenRepository,
            IUnitOfWork unitOfWork,
            ILoggerManager<JwtService> loggerManager)
        {
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _loggerManager = loggerManager;
        }

        public async Task<LoginResponseDto> GenerateTokensAsync(ApplicationUser user)
        {
            await RevokeAllForUserAsync(user.Id);

            var accessToken = await GenerateAccessTokenAsync(user);
            var refreshToken = await GenerateRefreshTokenAsync(user);

            await _unitOfWork.SaveChangesAsync();

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                User = new ApplicationUserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    FullName = user.FullName ?? string.Empty,
                    Roles = (await _userManager.GetRolesAsync(user)).ToArray()
                }
            };
        }

        public async Task<RefreshTokenResponseDto> RefreshTokenAsync(string? accessToken, string refreshToken)
        {
            var tokenEntity = await _refreshTokenRepository.Query(tracking: true)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.ExpiryDate < DateTime.UtcNow)
                throw new SecurityTokenException("Refresh token is invalid or expired.");

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                try
                {
                    var principal = GetPrincipalFromExpiredToken(accessToken);
                    var claimUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(claimUserId) && claimUserId != tokenEntity.UserId)
                        throw new SecurityTokenException("Access token does not match refresh token owner.");
                }
                catch (SecurityTokenException)
                {
                    throw;
                }
                catch
                {
                    // Malformed / unreadable access token — continue with refresh token alone.
                }
            }

            var user = await _userManager.FindByIdAsync(tokenEntity.UserId);
            if (user == null)
                throw new SecurityTokenException("User not found.");

            tokenEntity.IsRevoked = true;
            tokenEntity.RevokedDate = DateTime.UtcNow;
            tokenEntity.ReasonRevoked = "Replaced by new token";
            _refreshTokenRepository.Update(tokenEntity);

            var newAccessToken = await GenerateAccessTokenAsync(user);
            var newRefreshToken = await GenerateRefreshTokenAsync(user);

            await _unitOfWork.SaveChangesAsync();

            return new RefreshTokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
            };
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, string reason = "Revoked by user")
        {
            var tokenEntity = await _refreshTokenRepository.Query(tracking: true)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (tokenEntity == null)
                return false;

            tokenEntity.IsRevoked = true;
            tokenEntity.RevokedDate = DateTime.UtcNow;
            tokenEntity.ReasonRevoked = reason;

            _refreshTokenRepository.Update(tokenEntity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private async Task RevokeAllForUserAsync(string userId, string reason = "Superseded by new login")
        {
            var active = await _refreshTokenRepository.Query(tracking: true)
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            if (active.Count == 0) return;

            var now = DateTime.UtcNow;
            foreach (var token in active)
            {
                token.IsRevoked = true;
                token.RevokedDate = now;
                token.ReasonRevoked = reason;
            }
        }

        private async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new("FullName", user.FullName ?? string.Empty)
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task<string> GenerateRefreshTokenAsync(ApplicationUser user)
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                IsRevoked = false,
                CreatedDate = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);
            return refreshToken;
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.SecretKey)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token.");
            }

            return principal;
        }
    }
}
