using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ModularSaaS.Security.Abstractions;
using ModularSaaS.Security.Abstractions.Constants;

namespace ModularSaaS.Security.AspNetCore.Identity;

public sealed class AmbientSecurityContextAccessor(IHttpContextAccessor httpContextAccessor) : ISecurityContextAccessor
{
    private static readonly AsyncLocal<SecurityContextSnapshot?> CurrentSnapshot = new();

    public ICurrentUser CurrentUser
    {
        get
        {
            var snapshot = CurrentSnapshot.Value;
            if (snapshot is not null)
            {
                return new SnapshotCurrentUser(snapshot);
            }

            return new HttpCurrentUser(httpContextAccessor);
        }
    }

    public IDisposable BeginScope(SecurityContextSnapshot snapshot)
    {
        var previous = CurrentSnapshot.Value;
        CurrentSnapshot.Value = snapshot;
        return new ScopeDisposable(() => CurrentSnapshot.Value = previous);
    }

    private sealed class ScopeDisposable(Action onDispose) : IDisposable
    {
        private Action? _onDispose = onDispose;

        public void Dispose()
        {
            Interlocked.Exchange(ref _onDispose, null)?.Invoke();
        }
    }

    private sealed class SnapshotCurrentUser : ICurrentUser
    {
        public SnapshotCurrentUser(SecurityContextSnapshot snapshot)
        {
            UserId = snapshot.UserId;
            TenantId = snapshot.TenantId;

            var claims = new List<Claim>();
            if (snapshot.UserId.HasValue)
            {
                claims.Add(new Claim(SecurityClaimTypes.Subject, snapshot.UserId.Value.ToString()));
            }

            if (snapshot.TenantId.HasValue)
            {
                claims.Add(new Claim(SecurityClaimTypes.TenantId, snapshot.TenantId.Value.ToString()));
            }

            if (!string.IsNullOrWhiteSpace(snapshot.Email))
            {
                claims.Add(new Claim(SecurityClaimTypes.Email, snapshot.Email));
            }

            if (snapshot.Roles is not null)
            {
                foreach (var role in snapshot.Roles)
                {
                    claims.Add(new Claim(SecurityClaimTypes.Role, role));
                }
            }

            if (snapshot.IsPlatformAdmin)
            {
                claims.Add(new Claim(SecurityClaimTypes.Role, SecurityRoles.PlatformAdmin));
                claims.Add(new Claim(SecurityClaimTypes.Scope, "Platform"));
            }

            var identity = new ClaimsIdentity(claims, "AmbientScope");
            Principal = new ClaimsPrincipal(identity);
        }

        public Guid? UserId { get; }

        public Guid? TenantId { get; }

        public ClaimsPrincipal? Principal { get; }
    }
}
