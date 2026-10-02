using ModularSaaS.Domain.Tenancy.Enums;

namespace ModularSaaS.Application.Tenancy.Models;

public sealed record CreateTenantInput(
    string Name,
    string Identifier,
    string AdminEmail,
    string AdminPassword,
    string AdminFirstName,
    string AdminLastName,
    TenantPlan Plan = TenantPlan.Free);

public sealed record TenantResult(
    Guid Id,
    string Name,
    string Identifier,
    TenantStatus Status,
    TenantPlan Plan,
    DateTimeOffset CreatedAtUtc);
