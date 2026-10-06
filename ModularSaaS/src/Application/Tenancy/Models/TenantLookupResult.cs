using ModularSaaS.Domain.Tenancy.Enums;

namespace ModularSaaS.Application.Tenancy.Models;

public sealed record TenantLookupResult(
    Guid Id,
    string Identifier,
    string Name,
    TenantStatus Status,
    bool IsDeleted);
