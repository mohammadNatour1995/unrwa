using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Interfaces.Logging;
using Domain.Dtos.Users;
using Domain.Entities.Users;
using Infrastructure.Persistence;

namespace Application.Services.Users;

public class ApplicationUserManager : UserManager<ApplicationUser>
{
    private readonly ILoggerManager<ApplicationUserManager> _loggerManager;
    private readonly ApplicationDbContext _context;

    public ApplicationUserManager(
        ApplicationDbContext context,
        IServiceProvider services,
        IdentityErrorDescriber errors,
        ILookupNormalizer keyNormalizer,
        IUserStore<ApplicationUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<UserManager<ApplicationUser>> logger,
        IPasswordHasher<ApplicationUser> passwordHasher,
        ILoggerManager<ApplicationUserManager> loggerManager,
        IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _loggerManager = loggerManager;
        _context = context;
    }

    // Used by CurrentUserMiddleware to populate ICurrentUser from the JWT subject claim.
    public async Task<ApplicationUserDto?> FindCurrentUserAsync(string id)
    {
        var user = await Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user is null)
            return null;

        return new ApplicationUserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            FullName = user.FullName ?? string.Empty,
            IsActive = user.IsActive
        };
    }

    // Single JOIN query replacing per-user GetRolesAsync calls (N+1 fix).
    public async Task<Dictionary<string, string>> GetUserRoleMapAsync(IEnumerable<string> userIds)
    {
        var ids = userIds.ToList();
        var pairs = await (
            from ur in _context.Set<IdentityUserRole<string>>()
            join r in _context.Roles on ur.RoleId equals r.Id
            where ids.Contains(ur.UserId)
            select new { ur.UserId, r.Name }
        ).ToListAsync();

        return pairs
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.First().Name ?? string.Empty);
    }

    // Returns IDs of all users assigned to a specific role (single query, no N+1).
    public async Task<List<string>> GetUserIdsInRoleAsync(string normalizedRoleName)
    {
        var roleId = await _context.Roles
            .Where(r => r.NormalizedName == normalizedRoleName)
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        if (roleId == null) return [];

        return await _context.Set<IdentityUserRole<string>>()
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .ToListAsync();
    }
}
