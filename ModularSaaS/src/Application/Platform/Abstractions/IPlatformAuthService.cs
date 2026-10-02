using ModularSaaS.Application.Platform.Models;
using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Application.Platform.Abstractions;

public interface IPlatformAuthService
{
    public Task<Result<PlatformAuthResult>> LoginAsync(PlatformLoginInput input, CancellationToken ct = default);

    public Task<Result<ImpersonationResult>> ImpersonateAsync(Guid targetTenantId, ImpersonateInput input, CancellationToken ct = default);
}
