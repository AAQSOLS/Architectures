namespace ModularSaaS.Application.Shared.Abstractions;

public interface ITenantSetter
{
    public void SetTenant(Guid? tenantId, bool isPlatformScope = false, bool isImpersonated = false, Guid? impersonatedBy = null);
}
