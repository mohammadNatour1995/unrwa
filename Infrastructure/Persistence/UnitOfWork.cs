
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces;
using Domain.Interfaces.Users;

namespace Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ICurrentUser _currentUser;
    private readonly ApplicationDbContext _applicationDbContext;

    public UnitOfWork(ICurrentUser currentUser, ApplicationDbContext applicationDbContext)
    {
        _currentUser = currentUser;
        _applicationDbContext = applicationDbContext;
    }

    // EnableRetryOnFailure requires the whole begin/commit unit to run inside the
    // execution strategy so a dropped connection mid-transaction can be retried from scratch.
    public async Task ExecuteInTransactionAsync(Func<Task> operation, bool hardDelete = false)
    {
        var strategy = _applicationDbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
            try
            {
                await operation();
                await _applicationDbContext.SaveChangesAsync(_currentUser, hardDelete);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public async Task SaveChangesAsync(bool hardDelete = false)
    {
        await _applicationDbContext.SaveChangesAsync(_currentUser, hardDelete);
    }
}
