using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Application.Identity.Abstractions;

public interface IIdentityReader
{
    public Task<PagedResult<UserListItem>> ListUsersAsync(Guid tenantId, PageRequest page, CancellationToken ct = default);
}
