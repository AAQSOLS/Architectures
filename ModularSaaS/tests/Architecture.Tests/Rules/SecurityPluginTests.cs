using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using ModularSaaS.Security;
using ModularSaaS.Security.Authorization;
using ModularSaaS.Security.Constants;
using ModularSaaS.Security.Cryptography;
using Xunit;

namespace ModularSaaS.Architecture.Tests.Rules;

public class SecurityPluginTests
{
    [Fact]
    public void BCryptPasswordHasher_Hashes_And_Verifies_Correctly()
    {
        var hasher = new BCryptPasswordHasher(workFactor: 10);
        var password = "SuperSecretPassword123!";

        var hash = hasher.Hash(password);
        var isValid = BCryptPasswordHasher.Verify(password, hash);
        var isInvalid = BCryptPasswordHasher.Verify("WrongPassword!", hash);

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
    public void HasPermissionAttribute_Configures_Policy()
    {
        var attribute = new HasPermissionAttribute("Users.Read");
        Assert.Equal("Users.Read", attribute.Permission);
        Assert.Equal("Users.Read", attribute.Policy);
    }

    [Fact]
    public async Task PermissionAuthorizationHandler_Authorizes_Matching_Claim()
    {
        var requirement = new PermissionRequirement("Users.Read");
        var claims = new[]
        {
            new Claim(SecurityClaimTypes.Permission, "Users.Read")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var context = new AuthorizationHandlerContext([requirement], principal, null);

        var services = new ServiceCollection().BuildServiceProvider();
        var handler = new PermissionAuthorizationHandler(services);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task PermissionAuthorizationHandler_Authorizes_PlatformAdmin()
    {
        var requirement = new PermissionRequirement("Tenants.Delete");
        var claims = new[]
        {
            new Claim(SecurityClaimTypes.Role, SecurityRoles.PlatformAdmin)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var context = new AuthorizationHandlerContext([requirement], principal, null);

        var services = new ServiceCollection().BuildServiceProvider();
        var handler = new PermissionAuthorizationHandler(services);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
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
