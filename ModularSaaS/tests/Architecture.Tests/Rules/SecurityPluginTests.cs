using System.Security.Claims;
using ModularSaaS.Security.Abstractions;
using ModularSaaS.Security.Abstractions.Constants;
using ModularSaaS.Security.Abstractions.Extensions;
using ModularSaaS.Security.Core.Hashing;
using ModularSaaS.Security.Core.Tokens;
using Xunit;

namespace ModularSaaS.Architecture.Tests.Rules;

public class SecurityPluginTests
{
    private const string TestSigningKey = "ThisIsASecretKeyForTestingPurposesThatIsAtLeast32BytesLong!";

    [Fact]
    public void BCryptPasswordHasher_Hashes_And_Verifies_Correctly()
    {
        var hasher = new BCryptPasswordHasher(workFactor: 10);
        var password = "SuperSecretPassword123!";

        var hash = hasher.Hash(password);
        var isValid = hasher.Verify(password, hash);
        var isInvalid = hasher.Verify("WrongPassword!", hash);

        Assert.True(isValid);
        Assert.False(isInvalid);
    }

    [Fact]
    public void BCryptPasswordHasher_Detects_NeedsRehash_When_WorkFactor_Upgrades()
    {
        var oldHasher = new BCryptPasswordHasher(workFactor: 10);
        var newHasher = new BCryptPasswordHasher(workFactor: 12);
        var password = "SuperSecretPassword123!";

        var oldHash = oldHasher.Hash(password);

        Assert.False(oldHasher.NeedsRehash(oldHash));
        Assert.True(newHasher.NeedsRehash(oldHash));
        Assert.Equal(PasswordVerificationResult.SuccessRehashNeeded, newHasher.VerifyWithRehashCheck(password, oldHash));
    }

    [Fact]
    public void SecureRandomGenerator_Generates_Valid_Tokens_And_ConstantTimeEquals()
    {
        var generator = new SecureRandomGenerator();

        var urlToken = generator.CreateUrlSafeToken(32);
        var hexToken = generator.CreateHexString(32);
        var numericCode = generator.CreateNumericCode(6);

        Assert.False(string.IsNullOrWhiteSpace(urlToken));
        Assert.Equal(64, hexToken.Length);
        Assert.Equal(6, numericCode.Length);
        Assert.True(int.TryParse(numericCode, out _));

        Assert.True(generator.ConstantTimeEquals(urlToken, urlToken));
        Assert.False(generator.ConstantTimeEquals(urlToken, hexToken));
    }

    [Fact]
    public void JsonWebTokenService_Creates_And_Validates_Token()
    {
        var options = new JwtServiceOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SigningKey = TestSigningKey,
            DefaultLifetime = TimeSpan.FromMinutes(15),
            ClockSkew = TimeSpan.Zero
        };

        var service = new JsonWebTokenService(options);
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var descriptor = new TokenDescriptor(
            UserId: userId,
            TenantId: tenantId,
            Email: "user@test.com",
            Name: "Test User",
            Roles: ["Member", "Admin"],
            Permissions: ["Users.Read", "Users.Write"]);

        var tokenResult = service.CreateToken(descriptor);
        Assert.NotNull(tokenResult.Token);
        Assert.NotNull(tokenResult.TokenId);

        var principal = service.ValidateToken(tokenResult.Token);
        Assert.NotNull(principal);

        var sub = principal.FindFirst(SecurityClaimTypes.Subject)?.Value;
        var tid = principal.FindFirst(SecurityClaimTypes.TenantId)?.Value;
        var email = principal.FindFirst(SecurityClaimTypes.Email)?.Value;

        Assert.Equal(userId.ToString(), sub);
        Assert.Equal(tenantId.ToString(), tid);
        Assert.Equal("user@test.com", email);
    }

    [Fact]
    public void JsonWebTokenService_Rejects_Invalid_Signature()
    {
        var options1 = new JwtServiceOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SigningKey = TestSigningKey
        };

        var options2 = new JwtServiceOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SigningKey = "DifferentSigningKeyThatIsAlsoAtLeast32BytesLong!"
        };

        var service1 = new JsonWebTokenService(options1);
        var service2 = new JsonWebTokenService(options2);

        var token = service1.CreateToken(new TokenDescriptor(Guid.NewGuid(), Guid.NewGuid())).Token;

        var principal = service2.ValidateToken(token);
        Assert.Null(principal);
    }

    [Fact]
    public async Task InMemoryTokenRevocationRegistry_Tracks_Revoked_Tokens()
    {
        var registry = new InMemoryTokenRevocationRegistry();
        var tokenId = Guid.NewGuid().ToString();

        Assert.False(await registry.IsRevokedAsync(tokenId));

        await registry.RevokeAsync(tokenId, DateTimeOffset.UtcNow.AddMinutes(5));

        Assert.True(await registry.IsRevokedAsync(tokenId));
    }

    [Fact]
    public void CurrentUserExtensions_Extracts_Claims_Without_Interface_Bloat()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var claims = new[]
        {
            new Claim(SecurityClaimTypes.Subject, userId.ToString()),
            new Claim(SecurityClaimTypes.TenantId, tenantId.ToString()),
            new Claim(SecurityClaimTypes.Email, "admin@platform.com"),
            new Claim(SecurityClaimTypes.Role, SecurityRoles.PlatformAdmin),
            new Claim(SecurityClaimTypes.Permission, "Tenants.Read")
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var user = new TestCurrentUser(userId, tenantId, principal);

        Assert.True(user.IsAuthenticated());
        Assert.Equal("admin@platform.com", user.GetEmail());
        Assert.True(user.HasRole(SecurityRoles.PlatformAdmin));
        Assert.True(user.IsPlatformAdmin());
        Assert.Contains("Tenants.Read", user.GetPermissions());
    }

    [Fact]
    public void JwtTokenService_HashToken_Produces_Deterministic_Sha256_Hash()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new ModularSaaS.Infrastructure.Security.JwtOptions
        {
            Secret = "SuperSecretKeyForTestingAtLeast32BytesLong!",
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        });
        var clock = new ModularSaaS.Infrastructure.Time.SystemClock();
        var service = new ModularSaaS.Infrastructure.Security.JwtTokenService(options, clock);

        var token = "MySecureRandomResetToken1234567890";
        var hash1 = service.HashToken(token);
        var hash2 = service.HashToken(token);
        var differentHash = service.HashToken(token + "different");

        Assert.Equal(64, hash1.Length);
        Assert.Equal(hash1, hash2);
        Assert.NotEqual(hash1, differentHash);
    }

    [Fact]
    public async Task PermissionEvaluator_Evaluates_Permissions_From_Cache()
    {
        using var memoryCache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var readerMock = new TestUserPermissionReader(new HashSet<string> { "Users.Read", "Users.Write" });
        services.AddSingleton<ModularSaaS.Application.Identity.Abstractions.IUserPermissionReader>(readerMock);
        var sp = services.BuildServiceProvider();

        var cache = new ModularSaaS.Infrastructure.Caching.PermissionCache(memoryCache, sp);
        var tenantContext = new ModularSaaS.Infrastructure.Tenancy.TenantContext();
        tenantContext.SetTenant(tenantId);

        var currentUser = new TestAppCurrentUser(userId);
        var evaluator = new ModularSaaS.Infrastructure.Security.PermissionEvaluator(cache, tenantContext, currentUser);

        Assert.True(await evaluator.HasPermissionAsync("Users.Read"));
        Assert.True(await evaluator.HasPermissionAsync("Users.Write"));
        Assert.False(await evaluator.HasPermissionAsync("Users.Delete"));
        Assert.True(await evaluator.HasAnyPermissionAsync(["Users.Delete", "Users.Read"]));
        Assert.False(await evaluator.HasAllPermissionsAsync(["Users.Read", "Users.Delete"]));
    }

    private sealed class TestAppCurrentUser(Guid userId) : ModularSaaS.Application.Shared.Abstractions.ICurrentUser
    {
        public Guid? UserId { get; } = userId;

        public string? Email => "test@test.com";

        public bool IsAuthenticated => true;

        public bool IsPlatformAdmin => false;

        public bool IsImpersonated => false;

        public Guid? ActorId => null;

        public IReadOnlyList<string> Roles => [];
    }

    private sealed class TestUserPermissionReader(IReadOnlySet<string> permissions) : ModularSaaS.Application.Identity.Abstractions.IUserPermissionReader
    {
        public Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(Guid tenantId, Guid userId, CancellationToken ct = default) =>
            Task.FromResult(permissions);
    }

    private sealed class TestCurrentUser(Guid? userId, Guid? tenantId, ClaimsPrincipal? principal) : ICurrentUser
    {
        public Guid? UserId { get; } = userId;

        public Guid? TenantId { get; } = tenantId;

        public ClaimsPrincipal? Principal { get; } = principal;
    }
}
