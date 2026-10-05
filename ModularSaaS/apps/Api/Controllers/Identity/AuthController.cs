using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ModularSaaS.Api.Common;
using ModularSaaS.Api.Extensions;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Constants;

namespace ModularSaaS.Api.Controllers.Identity;

[ApiVersion(ApiVersionConstants.V1String)]
[Route(ApiRoutes.Auth.Prefix)]
public sealed class AuthController(IAuthService authService) : BaseApiController
{
    [HttpPost(ApiRoutes.Auth.Login)]
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicyName)]
    public async Task<IActionResult> Login([FromBody] LoginInput input, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await authService.LoginAsync(input, ip, ct);
        return HandleResult(result);
    }

    [HttpPost(ApiRoutes.Auth.Refresh)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenInput input, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await authService.RefreshTokenAsync(input, ip, ct);
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost(ApiRoutes.Auth.Revoke)]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenInput input, CancellationToken ct)
    {
        var result = await authService.RevokeTokenAsync(input.RefreshToken, ct);
        return HandleResult(result);
    }

    [HttpPost(ApiRoutes.Auth.ForgotPassword)]
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicyName)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordInput input, CancellationToken ct)
    {
        var result = await authService.ForgotPasswordAsync(input, ct);
        return HandleResult(result);
    }

    [HttpPost(ApiRoutes.Auth.ResetPassword)]
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicyName)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordInput input, CancellationToken ct)
    {
        var result = await authService.ResetPasswordAsync(input, ct);
        return HandleResult(result);
    }
}
