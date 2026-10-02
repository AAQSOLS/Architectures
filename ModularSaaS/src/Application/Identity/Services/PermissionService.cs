using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Application.Identity.Services;

internal sealed class PermissionService(IPermissionRepository permissionRepository) : IPermissionService
{
    public async Task<Result<IReadOnlyList<PermissionGroupResult>>> GetPermissionMatrixAsync(CancellationToken ct = default)
    {
        var all = await permissionRepository.GetAllAsync(ct);

        var groups = all
            .GroupBy(p => p.Module)
            .OrderBy(g => g.Key)
            .Select(g => new PermissionGroupResult(
                g.Key,
                g.Select(p => new PermissionItem(p.Id, p.Code, p.Name, p.Description))
                 .OrderBy(p => p.Code)
                 .ToList()))
            .ToList();

        return groups;
    }
}
