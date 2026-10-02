using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Application.Tenancy.Errors;

public static class TenancyErrors
{
    public static Error NotFound => Error.NotFound(
        "Tenant.NotFound",
        "The requested tenant was not found.");

    public static Error IdentifierExists => Error.Conflict(
        "Tenant.IdentifierExists",
        "A tenant with this identifier already exists.");

    public static Error Validation(string message) => Error.Validation(
        "Tenant.Validation",
        message);
}
