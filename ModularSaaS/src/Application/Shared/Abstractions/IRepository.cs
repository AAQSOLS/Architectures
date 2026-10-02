using ModularSaaS.Domain.Shared;

namespace ModularSaaS.Application.Shared.Abstractions;

public interface IRepository<T> where T : class, IAggregateRoot
{
    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    public Task AddAsync(T entity, CancellationToken ct = default);

    public void Update(T entity);

    public void Remove(T entity);
}
