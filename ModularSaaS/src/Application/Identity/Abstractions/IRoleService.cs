using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Application.Identity.Abstractions;

public interface IRoleService
{
    public Task<Result<RoleResult>> CreateRoleAsync(CreateRoleInput input, CancellationToken ct = default);

    public Task<Result<RoleDetailsResult>> GetByIdAsync(Guid id, CancellationToken ct = default);

    public Task<Result<IReadOnlyList<RoleResult>>> ListRolesAsync(CancellationToken ct = default);

    public Task<Result<RoleResult>> UpdateRoleAsync(Guid id, UpdateRoleInput input, CancellationToken ct = default);

    public Task<Result> DeleteRoleAsync(Guid id, CancellationToken ct = default);
}
