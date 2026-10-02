using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ModularSaaS.Api.Common;
using ModularSaaS.Api.Contracts.Identity;
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
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var input = request.Adapt<LoginInput>();
        var result = await authService.LoginAsync(input, ip, ct);
        return HandleResult<AuthTokensResult, AuthResponse>(result);
    }

    [HttpPost(ApiRoutes.Auth.Refresh)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var input = request.Adapt<RefreshTokenInput>();
        var result = await authService.RefreshTokenAsync(input, ip, ct);
        return HandleResult<AuthTokensResult, AuthResponse>(result);
    }

    [Authorize]
    [HttpPost(ApiRoutes.Auth.Revoke)]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await authService.RevokeTokenAsync(request.RefreshToken, ct);
        return HandleResult(result);
    }

    [HttpPost(ApiRoutes.Auth.ForgotPassword)]
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicyName)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        var input = request.Adapt<ForgotPasswordInput>();
        var result = await authService.ForgotPasswordAsync(input, ct);
        return HandleResult(result);
    }

    [HttpPost(ApiRoutes.Auth.ResetPassword)]
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicyName)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var input = request.Adapt<ResetPasswordInput>();
        var result = await authService.ResetPasswordAsync(input, ct);
        return HandleResult(result);
    }
}

