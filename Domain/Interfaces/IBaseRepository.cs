using Domain.Entities;

namespace Domain.Interfaces;

public interface IEFBaseRepository<T> where T : BaseEntity
{
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<T?> FindAsync(Guid id, CancellationToken cancellationToken = default);
    IQueryable<T> Query(bool tracking = false);
    T Update(T entity);
    T Delete(T entity);
}

public interface IEFBaseLookupRepository<T> where T : IBaseLookup
{
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<T?> FindAsync(Guid id, CancellationToken cancellationToken = default);
    IQueryable<T> Query(bool tracking = false);
    T Update(T entity);
    T Delete(T entity);
}
