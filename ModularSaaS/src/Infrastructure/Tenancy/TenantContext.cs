using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Application.Shared.Exceptions;

namespace ModularSaaS.Infrastructure.Tenancy;

internal sealed class TenantContext : ITenantContext, ITenantSetter
{
    public Guid? TenantId { get; set; }

    public bool IsPlatformScope { get; set; }

    public bool IsImpersonated { get; set; }

    public Guid? ImpersonatedBy { get; set; }

    public Guid RequireTenantId()
    {
        if (TenantId.HasValue && TenantId.Value != Guid.Empty)
        {
            return TenantId.Value;
        }

        throw new TenantRequiredException();
    }

    public void SetTenant(Guid? tenantId, bool isPlatformScope = false, bool isImpersonated = false, Guid? impersonatedBy = null)
    {
        TenantId = tenantId;
        IsPlatformScope = isPlatformScope;
        IsImpersonated = isImpersonated;
        ImpersonatedBy = impersonatedBy;
    }
}
