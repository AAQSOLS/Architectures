using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Domain.Shared;

namespace ModularSaaS.Infrastructure.Persistence.Repositories;

internal abstract class EfRepository<T>(AppDbContext dbContext) : IRepository<T> where T : class, IAggregateRoot
{
    protected readonly AppDbContext DbContext = dbContext;

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await DbContext.Set<T>().FindAsync([id], ct);
    }

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await DbContext.Set<T>().AddAsync(entity, ct);
    }

    public virtual void Update(T entity)
    {
        DbContext.Set<T>().Update(entity);
    }

    public virtual void Remove(T entity)
    {
        DbContext.Set<T>().Remove(entity);
    }
}
