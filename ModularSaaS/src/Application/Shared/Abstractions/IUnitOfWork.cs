namespace ModularSaaS.Application.Shared.Abstractions;

public interface IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default);
}
