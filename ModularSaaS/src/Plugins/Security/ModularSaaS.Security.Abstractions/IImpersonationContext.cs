namespace ModularSaaS.Security.Abstractions;

public interface IImpersonationContext
{
    public bool IsImpersonated { get; }

    public Guid? ImpersonatorUserId { get; }

    public string? ImpersonatorEmail { get; }
}
