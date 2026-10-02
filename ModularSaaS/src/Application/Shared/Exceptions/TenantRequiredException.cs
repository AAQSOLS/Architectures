namespace ModularSaaS.Application.Shared.Exceptions;

public sealed class TenantRequiredException(string message = "A tenant identifier is required for this operation.")
    : Exception(message)
{
}
