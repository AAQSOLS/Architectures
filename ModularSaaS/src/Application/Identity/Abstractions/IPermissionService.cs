using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Application.Identity.Abstractions;

public interface IPermissionService
{
    public Task<Result<IReadOnlyList<PermissionGroupResult>>> GetPermissionMatrixAsync(CancellationToken ct = default);
}
