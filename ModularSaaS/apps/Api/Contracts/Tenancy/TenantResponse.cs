using ModularSaaS.Domain.Tenancy.Enums;

namespace ModularSaaS.Api.Contracts.Tenancy;

public sealed record TenantResponse(
    Guid Id,
    string Name,
    string Identifier,
    TenantStatus Status,
    TenantPlan Plan,
    DateTimeOffset CreatedAtUtc);

