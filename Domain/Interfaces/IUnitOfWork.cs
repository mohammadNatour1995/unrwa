namespace Domain.Interfaces;

public interface IUnitOfWork
{
    Task ExecuteInTransactionAsync(Func<Task> operation, bool hardDelete = false);

    Task SaveChangesAsync(bool hardDelete = false);
}
