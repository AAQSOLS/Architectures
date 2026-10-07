using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModularSaaS.Api.Common;
using ModularSaaS.Application.Platform.Abstractions;
using ModularSaaS.Application.Platform.Models;
using ModularSaaS.Application.Shared.Constants;

namespace ModularSaaS.Api.Controllers.Platform;

[ApiVersion(ApiVersionConstants.V1String)]
[Authorize(Roles = AppRoles.PlatformAdmin)]
[Route(ApiRoutes.PlatformAdmins.Prefix)]
public sealed class PlatformAdminsController(IPlatformUserService platformUserService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> ListAdmins(CancellationToken ct)
    {
        var result = await platformUserService.ListAdminsAsync(ct);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdmin([FromBody] CreatePlatformAdminInput input, CancellationToken ct)
    {
        var result = await platformUserService.CreateAdminAsync(input, ct);
        return HandleResult(result);
    }

    [HttpPost(ApiRoutes.PlatformAdmins.Deactivate)]
    public async Task<IActionResult> DeactivateAdmin(Guid id, CancellationToken ct)
    {
        var result = await platformUserService.DeactivateAdminAsync(id, ct);
        return HandleResult(result);
    }

    [HttpPost(ApiRoutes.PlatformAdmins.Activate)]
    public async Task<IActionResult> ActivateAdmin(Guid id, CancellationToken ct)
    {
        var result = await platformUserService.ActivateAdminAsync(id, ct);
        return HandleResult(result);
    }
}
