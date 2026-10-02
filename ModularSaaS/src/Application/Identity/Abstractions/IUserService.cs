using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Application.Identity.Abstractions;

public interface IUserService : IUserRegistrationService, IUserImpersonationService
{
    public Task<Result<UserDetailsResult>> GetCurrentUserAsync(CancellationToken ct = default);

    public Task<Result<UserDetailsResult>> GetByIdAsync(Guid id, CancellationToken ct = default);

    public Task<Result<PagedResult<UserListItem>>> ListUsersAsync(PageRequest page, CancellationToken ct = default);

    public Task<Result> ChangePasswordAsync(ChangePasswordInput input, CancellationToken ct = default);

    public Task<Result> AssignRoleAsync(Guid userId, Guid roleId, DateTimeOffset? expiresAtUtc = null, CancellationToken ct = default);

    public Task<Result> RemoveRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default);
}
