using System.Text.Json.Serialization;
using ModularSaaS.Domain.Shared;

namespace ModularSaaS.Domain.Identity.Events;

[method: JsonConstructor]
public sealed record UserCreatedDomainEvent(
    Guid UserId,
    Guid TenantId,
    string Email,
    string FirstName,
    string LastName,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : ITenantEvent
{
    public UserCreatedDomainEvent(
        Guid userId,
        Guid tenantId,
        string email,
        string firstName,
        string lastName)
        : this(userId, tenantId, email, firstName, lastName, Guid.NewGuid(), TimeProvider.System.GetUtcNow())
    {
    }
}
