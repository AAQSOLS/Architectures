using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Application.Shared.Exceptions;

namespace ModularSaaS.Testing.Shared.Host.Fakes;

public sealed class TestTenantContext : ITenantContext, ITenantSetter
{
    public TestTenantContext(Guid? initialTenantId = null, bool isPlatformScope = false)
    {
        TenantId = initialTenantId;
        IsPlatformScope = isPlatformScope;
    }

    public Guid? TenantId { get; private set; }

    public bool IsPlatformScope { get; private set; }

    public bool IsImpersonated { get; private set; }

    public Guid? ImpersonatedBy { get; private set; }

    public Guid RequireTenantId()
    {
        if (!TenantId.HasValue || TenantId.Value == Guid.Empty)
        {
            throw new TenantRequiredException();
        }

        return TenantId.Value;
    }

    public void SetTenant(Guid? tenantId, bool isPlatformScope = false, bool isImpersonated = false, Guid? impersonatedBy = null)
    {
        TenantId = tenantId;
        IsPlatformScope = isPlatformScope;
        IsImpersonated = isImpersonated;
        ImpersonatedBy = impersonatedBy;
    }
}
