using ModularSaaS.Domain.Identity;

namespace ModularSaaS.Application.Identity.Abstractions;

public interface IPermissionRepository
{
    public Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct = default);

    public Task<IReadOnlyList<Permission>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken ct = default);

    public Task<Permission?> GetByCodeAsync(string code, CancellationToken ct = default);
}
