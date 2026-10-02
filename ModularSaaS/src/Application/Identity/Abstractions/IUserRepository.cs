using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Domain.Identity;

namespace ModularSaaS.Application.Identity.Abstractions;

public interface IUserRepository : IRepository<User>, IUserPermissionReader
{
    public Task<User?> GetByEmailAsync(Guid tenantId, string email, CancellationToken ct = default);

    public Task<bool> EmailExistsAsync(Guid tenantId, string email, CancellationToken ct = default);

    public Task<IReadOnlyList<string>> GetUserRoleNamesAsync(Guid userId, CancellationToken ct = default);

    public Task<IReadOnlyList<Role>> GetUserRolesAsync(Guid userId, CancellationToken ct = default);

    public Task AssignRoleAsync(Guid userId, Guid roleId, Guid? assignedBy = null, DateTimeOffset? expiresAtUtc = null, CancellationToken ct = default);

    public Task RemoveRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default);

    public Task SetDirectPermissionAsync(Guid userId, Guid permissionId, bool isGranted, CancellationToken ct = default);
}
