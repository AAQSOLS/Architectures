namespace ModularSaaS.Application.Shared.Abstractions;

public interface ITenantContext
{
    public Guid? TenantId { get; }

    public bool IsPlatformScope { get; }

    public bool IsImpersonated { get; }

    public Guid? ImpersonatedBy { get; }

    public Guid RequireTenantId();
}
