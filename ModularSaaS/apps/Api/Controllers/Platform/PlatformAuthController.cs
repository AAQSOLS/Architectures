using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ModularSaaS.Api.Common;
using ModularSaaS.Application.Platform.Abstractions;
using ModularSaaS.Application.Platform.Models;
using ModularSaaS.Application.Shared.Constants;

namespace ModularSaaS.Api.Controllers.Platform;

[ApiVersion(ApiVersionConstants.V1String)]
[Route(ApiRoutes.PlatformAuth.Prefix)]
public sealed class PlatformAuthController(IPlatformAuthService platformAuthService) : BaseApiController
{
    [HttpPost(ApiRoutes.PlatformAuth.Login)]
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    public async Task<IActionResult> Login([FromBody] PlatformLoginInput input, CancellationToken ct)
    {
        var result = await platformAuthService.LoginAsync(input, ct);
        return HandleResult(result);
    }

    [HttpPost(ApiRoutes.PlatformAuth.Impersonate)]
    [Authorize(Roles = AppRoles.PlatformAdmin)]
    public async Task<IActionResult> Impersonate(Guid tenantId, [FromBody] ImpersonateInput input, CancellationToken ct)
    {
        var result = await platformAuthService.ImpersonateAsync(tenantId, input, ct);
        return HandleResult(result);
    }
}
