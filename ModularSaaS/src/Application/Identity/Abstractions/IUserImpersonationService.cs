using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Application.Identity.Abstractions;

public interface IUserImpersonationService
{
    public Task<Result<UserImpersonationData>> GetImpersonationDataAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
}
