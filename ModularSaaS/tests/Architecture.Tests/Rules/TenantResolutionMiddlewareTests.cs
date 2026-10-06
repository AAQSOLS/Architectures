using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularSaaS.Api.Common;
using ModularSaaS.Api.Middleware;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Application.Shared.Constants;
using ModularSaaS.Application.Tenancy.Abstractions;
using ModularSaaS.Application.Tenancy.Models;
using Xunit;

namespace ModularSaaS.Architecture.Tests.Rules;

public class TenantResolutionMiddlewareTests
{
    [Fact]
    public async Task Active_Tenant_Header_Sets_Tenant_And_Calls_Next()
    {
        var activeTenantId = Guid.NewGuid();
        var tenantSetter = new TestTenantSetter();
        var tenantLookup = new TestTenantLookupService(activeTenantId, isActive: true);

        var nextCalled = false;
        var middleware = new TenantResolutionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Request.Headers[AppHeaders.TenantId] = activeTenantId.ToString();

        await middleware.InvokeAsync(context, tenantSetter, tenantLookup);

        Assert.True(nextCalled);
        Assert.Equal(activeTenantId, tenantSetter.TenantId);
        Assert.False(tenantSetter.IsPlatformScope);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task Missing_Or_Inactive_Tenant_Header_Returns_403_Forbidden()
    {
        var inactiveTenantId = Guid.NewGuid();
        var tenantSetter = new TestTenantSetter();
        var tenantLookup = new TestTenantLookupService(inactiveTenantId, isActive: false);

        var nextCalled = false;
        var middleware = new TenantResolutionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Headers[AppHeaders.TenantId] = inactiveTenantId.ToString();

        await middleware.InvokeAsync(context, tenantSetter, tenantLookup);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.Equal(ProblemDetailsConstants.ContentType, context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body);
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status403Forbidden, problem.Status);
        Assert.Equal(ProblemDetailsConstants.TenantInactiveTitle, problem.Title);
    }

    [Fact]
    public async Task Authenticated_User_With_Inactive_Tenant_Claim_Returns_403_Forbidden()
    {
        var inactiveTenantId = Guid.NewGuid();
        var tenantSetter = new TestTenantSetter();
        var tenantLookup = new TestTenantLookupService(inactiveTenantId, isActive: false);

        var nextCalled = false;
        var middleware = new TenantResolutionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var claims = new[]
        {
            new Claim(AppClaimTypes.TenantId, inactiveTenantId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));

        await middleware.InvokeAsync(context, tenantSetter, tenantLookup);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task Platform_Endpoint_Skips_Tenant_Validation_And_Sets_Platform_Scope()
    {
        var tenantSetter = new TestTenantSetter();
        var tenantLookup = new TestTenantLookupService(Guid.NewGuid(), isActive: false);

        var nextCalled = false;
        var middleware = new TenantResolutionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/platform/auth/login";

        await middleware.InvokeAsync(context, tenantSetter, tenantLookup);

        Assert.True(nextCalled);
        Assert.Null(tenantSetter.TenantId);
        Assert.True(tenantSetter.IsPlatformScope);
    }

    [Fact]
    public async Task Request_Without_Tenant_Header_Passes_Through_With_Null_Tenant()
    {
        var tenantSetter = new TestTenantSetter();
        var tenantLookup = new TestTenantLookupService(Guid.NewGuid(), isActive: true);

        var nextCalled = false;
        var middleware = new TenantResolutionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Request.Path = "/health";

        await middleware.InvokeAsync(context, tenantSetter, tenantLookup);

        Assert.True(nextCalled);
        Assert.Null(tenantSetter.TenantId);
        Assert.False(tenantSetter.IsPlatformScope);
    }

    private sealed class TestTenantSetter : ITenantSetter
    {
        public Guid? TenantId { get; private set; }
        public bool IsPlatformScope { get; private set; }
        public bool IsImpersonated { get; private set; }
        public Guid? ImpersonatedBy { get; private set; }

        public void SetTenant(Guid? tenantId, bool isPlatformScope = false, bool isImpersonated = false, Guid? impersonatedBy = null)
        {
            TenantId = tenantId;
            IsPlatformScope = isPlatformScope;
            IsImpersonated = isImpersonated;
            ImpersonatedBy = impersonatedBy;
        }
    }

    private sealed class TestTenantLookupService(Guid activeTenantId, bool isActive) : ITenantLookupService
    {
        public Task<bool> IsTenantActiveAsync(Guid tenantId, CancellationToken ct = default)
        {
            return Task.FromResult(tenantId == activeTenantId && isActive);
        }

        public Task<TenantLookupResult?> FindByIdAsync(Guid tenantId, CancellationToken ct = default)
        {
            if (tenantId == activeTenantId && isActive)
            {
                return Task.FromResult<TenantLookupResult?>(new TenantLookupResult(
                    activeTenantId,
                    "test-tenant",
                    "Test Tenant",
                    ModularSaaS.Domain.Tenancy.Enums.TenantStatus.Active,
                    false));
            }

            return Task.FromResult<TenantLookupResult?>(null);
        }

        public Task InvalidateAsync(Guid tenantId, CancellationToken ct = default) => Task.CompletedTask;
    }
}
