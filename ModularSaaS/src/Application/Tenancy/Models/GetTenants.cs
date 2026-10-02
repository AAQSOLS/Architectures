using ModularSaaS.Application.Shared.Models;
using ModularSaaS.Domain.Tenancy.Enums;

namespace ModularSaaS.Application.Tenancy.Models;

public sealed record TenantListItem(
    Guid Id,
    string Name,
    string Identifier,
    TenantStatus Status,
    TenantPlan Plan,
    DateTimeOffset CreatedAtUtc);

public sealed record TenantFilter(
    TenantStatus? Status = null,
    string? SearchTerm = null,
    PageRequest? Page = null);
