using System.Security.Claims;

namespace ModularSaaS.Security.Abstractions;

public interface ICurrentUser
{
    public Guid? UserId { get; }

    public Guid? TenantId { get; }

    public ClaimsPrincipal? Principal { get; }
}
