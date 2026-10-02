namespace ModularSaaS.Application.Identity.Models;

public sealed record UserImpersonationData(
    Guid UserId,
    Guid TenantId,
    string Email,
    IReadOnlyList<string> Roles,
    IReadOnlySet<string> Permissions);
