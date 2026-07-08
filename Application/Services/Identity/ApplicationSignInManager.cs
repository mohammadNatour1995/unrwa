using Application.Services.Users;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services.Identity
{
    public class ApplicationSignInManager : SignInManager<ApplicationUser>
    {
        private readonly ApplicationUserManager _applicationUserManager;

        public ApplicationSignInManager(
            ApplicationUserManager applicationUserManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<ApplicationUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<ApplicationUser> confirmation)
            : base(applicationUserManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)

        {
            _applicationUserManager = applicationUserManager;
        }

        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var user = await _applicationUserManager.FindByNameAsync(userName);
            if (user == null)
            {
                return SignInResult.Failed;
            }

            if (await _applicationUserManager.IsLockedOutAsync(user))
            {
                return await LockedOut(user);
            }

            if (!user.IsActive)
            {
                return await LockedOut(user);
            }

            if (await _applicationUserManager.CheckPasswordAsync(user, password))
            {
                if (await _applicationUserManager.GetLockoutEnabledAsync(user))
                {
                    await _applicationUserManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                    return await LockedOut(user);
                }

                await _applicationUserManager.SetLockoutEnabledAsync(user, false);
                await ResetLockout(user);
                await _applicationUserManager.ResetAccessFailedCountAsync(user);
                return await SignInOrTwoFactorAsync(user, isPersistent);
            }

            if (lockoutOnFailure)
            {
                await _applicationUserManager.AccessFailedAsync(user);
                if (await _applicationUserManager.IsLockedOutAsync(user))
                {
                    return await LockedOut(user);
                }
            }

            return SignInResult.Failed;
        }

    }
}
