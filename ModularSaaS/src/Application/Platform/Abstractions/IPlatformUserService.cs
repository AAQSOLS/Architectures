using ModularSaaS.Application.Platform.Models;
using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Application.Platform.Abstractions;

public interface IPlatformUserService
{
    public Task<Result<IReadOnlyList<PlatformAdminResult>>> ListAdminsAsync(CancellationToken ct = default);

    public Task<Result<PlatformAdminResult>> CreateAdminAsync(CreatePlatformAdminInput input, CancellationToken ct = default);

    public Task<Result> DeactivateAdminAsync(Guid id, CancellationToken ct = default);

    public Task<Result> ActivateAdminAsync(Guid id, CancellationToken ct = default);
}
