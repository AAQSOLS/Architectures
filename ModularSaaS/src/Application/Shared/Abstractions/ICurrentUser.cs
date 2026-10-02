namespace ModularSaaS.Application.Shared.Abstractions;

public interface ICurrentUser
{
    public Guid? UserId { get; }

    public string? Email { get; }

    public bool IsAuthenticated { get; }

    public bool IsPlatformAdmin { get; }

    public bool IsImpersonated { get; }

    public Guid? ActorId { get; }

    public IReadOnlyList<string> Roles { get; }
}
