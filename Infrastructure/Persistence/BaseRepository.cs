
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Interfaces;

namespace Infrastructure.Persistence.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual T Delete(T entity)
    {
        entity.IsDeleted = true;
        _context.Set<T>().Update(entity);

        return entity;
    }

    public virtual async Task<T?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Set<T>().FindAsync(id, cancellationToken);
        if (entity is null || entity.IsDeleted)
            return null;

        return entity;
    }

    public virtual IQueryable<T> Query(bool tracking = false)
    {
        var query = _context.Set<T>()
            .Where(x => !x.IsDeleted);

        return tracking
            ? query
            : query.AsNoTracking();
    }

    public virtual T Update(T entity)
    {
        _context.Set<T>().Update(entity);
        return entity;
    }
}
public class BaseLookupRepository<T> : IBaseLookupRepository<T> where T : class, IBaseLookup
{
    protected readonly ApplicationDbContext _context;
    public BaseLookupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual T Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
        return entity;
    }


    public virtual async Task<T?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().FindAsync(id, cancellationToken);
    }
   
    public virtual IQueryable<T> Query(bool tracking = false)
    {
        var query = _context.Set<T>();
        return tracking
            ? query
            : query.AsNoTracking();
    }


    public virtual T Update(T entity)
    {
        _context.Set<T>().Update(entity);
        return entity;
    }
}
