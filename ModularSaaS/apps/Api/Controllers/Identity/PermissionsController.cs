using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModularSaaS.Api.Common;
using ModularSaaS.Api.Contracts.Identity;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Security.AspNetCore.Authorization;
using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Identity.Permissions;
using ModularSaaS.Application.Shared.Constants;

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
        return HandleResult<IReadOnlyList<PermissionGroupResult>, IReadOnlyList<PermissionGroupResponse>>(result);
    }
}

