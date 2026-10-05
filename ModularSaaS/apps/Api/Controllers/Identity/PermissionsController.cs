using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModularSaaS.Api.Common;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Identity.Permissions;
using ModularSaaS.Application.Shared.Constants;
using ModularSaaS.Security.Authorization;

namespace ModularSaaS.Api.Controllers.Identity;

[ApiVersion(ApiVersionConstants.V1String)]
[Authorize]
[Route(ApiRoutes.Permissions.Prefix)]
public sealed class PermissionsController(IPermissionService permissionService) : BaseApiController
{
    [HttpGet]
    [HasPermission(AppPermissions.Identity.PermissionsRead)]
    public async Task<IActionResult> GetPermissionMatrix(CancellationToken ct)
    {
        var result = await permissionService.GetPermissionMatrixAsync(ct);
        return HandleResult(result);
    }
}
