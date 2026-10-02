namespace ModularSaaS.Security.Abstractions;

public interface ISecurityContextAccessor
{
    public ICurrentUser CurrentUser { get; }

    public IDisposable BeginScope(SecurityContextSnapshot snapshot);
}
