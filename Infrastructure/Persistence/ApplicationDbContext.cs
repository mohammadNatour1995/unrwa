using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Constants;
using Domain.Entities;
using Domain.Entities.Auth;
using Domain.Entities.Lookup;
using Domain.Entities.Users;
using Domain.Interfaces.Users;
namespace Infrastructure.Persistence;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    // Identity
    public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public virtual DbSet<ApplicationRole> ApplicationRoles { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<Logs> Logs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Logs>(entity =>
        {
            entity.ToTable("Logs", "dbo");
            entity.Metadata.SetIsTableExcludedFromMigrations(true);
        });

        SeedData(builder);
    }
    public virtual async Task SaveChangesAsync(ICurrentUser currentUser, bool hardDelete = false)
    {
        ChangeTracker.DetectChanges();
        foreach (var entry in ChangeTracker.Entries().ToList())
        {
            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            if (entry.Entity is BaseEntity baseEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    baseEntity.CreateBy = currentUser.Info?.Id ?? "system";
                    baseEntity.CreateDate = DateTime.UtcNow;
                }
                if (entry.State == EntityState.Modified && !hardDelete)
                {
                    baseEntity.ModifiedBy = currentUser.Info?.Id;
                    baseEntity.ModifiedDate = DateTime.UtcNow;
                    entry.State = EntityState.Modified;
                }
                if (entry.State == EntityState.Deleted && !hardDelete)
                {
                    baseEntity.IsDeleted = true;
                    baseEntity.DeletedBy = currentUser.Info?.Id;
                    baseEntity.DeletedDate = DateTime.UtcNow;
                    entry.State = EntityState.Modified;
                }
            }
        }

        // Single SaveChanges call after all entries are processed.
        await base.SaveChangesAsync();
    }
    private void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationRole>().HasData(
            RoleSet.Administrator.ToEntity(),
            RoleSet.Admin.ToEntity()
            );

        modelBuilder.Entity<ApplicationUser>().HasData(
            UserSet.Administrator.ToEntity(),
            UserSet.Admin.ToEntity()
            );

        modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                UserId = UserSet.Administrator.Id,
                RoleId = RoleSet.Administrator.Id
            },
            new IdentityUserRole<string>
            {
                UserId = UserSet.Admin.Id,
                RoleId = RoleSet.Admin.Id
            }
            );
    }
}
