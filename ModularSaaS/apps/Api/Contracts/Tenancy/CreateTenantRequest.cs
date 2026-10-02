using ModularSaaS.Domain.Tenancy.Enums;

namespace ModularSaaS.Api.Contracts.Tenancy;

public sealed record CreateTenantRequest(
    string Name,
    string Identifier,
    string AdminEmail,
    string AdminPassword,
    string AdminFirstName,
    string AdminLastName,
    TenantPlan Plan = TenantPlan.Free);

