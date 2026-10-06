using ModularSaaS.Application.Shared.Abstractions;

namespace ModularSaaS.Testing.Shared.Host.Fakes;

public sealed class TestCurrentUser : ICurrentUser
{
    public TestCurrentUser(Guid? userId = null, string? email = null, bool isAuthenticated = false)
    {
        UserId = userId;
        Email = email;
        IsAuthenticated = isAuthenticated;
    }

    public Guid? UserId { get; set; }

    public string? Email { get; set; }

    public bool IsAuthenticated { get; set; }

    public bool IsPlatformAdmin { get; set; }

    public bool IsImpersonated { get; set; }

    public Guid? ActorId { get; set; }

    public IReadOnlyList<string> Roles { get; set; } = [];
}
