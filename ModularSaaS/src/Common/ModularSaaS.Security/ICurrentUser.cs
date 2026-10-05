using System.Security.Claims;

namespace ModularSaaS.Security;

public interface ICurrentUser
{
    public Guid? UserId { get; }

    public Guid? TenantId { get; }

    public ClaimsPrincipal? Principal { get; }
}
