using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Domain.Platform;

namespace ModularSaaS.Application.Platform.Abstractions;

public interface IPlatformUserRepository : IRepository<PlatformUser>
{
    public Task<PlatformUser?> GetByEmailAsync(string email, CancellationToken ct = default);

    public Task<bool> ExistsAsync(string email, CancellationToken ct = default);
}
