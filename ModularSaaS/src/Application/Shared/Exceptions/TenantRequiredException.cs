namespace ModularSaaS.Application.Shared.Exceptions;

public sealed class TenantRequiredException : Exception
{
    public TenantRequiredException()
        : base("A tenant identifier is required for this operation.")
    {
    }

    public TenantRequiredException(string message)
        : base(message)
    {
    }

    public TenantRequiredException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
