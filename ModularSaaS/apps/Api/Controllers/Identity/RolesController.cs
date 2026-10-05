using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModularSaaS.Api.Common;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Identity.Permissions;
using ModularSaaS.Application.Shared.Constants;
using ModularSaaS.Security.AspNetCore.Authorization;

namespace ModularSaaS.Api.Controllers.Identity;

[ApiVersion(ApiVersionConstants.V1String)]
[Authorize]
[Route(ApiRoutes.Roles.Prefix)]
public sealed class RolesController(IRoleService roleService) : BaseApiController
{
    [HttpPost]
    [HasPermission(AppPermissions.Identity.RolesWrite)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleInput input, CancellationToken ct)
    {
        var result = await roleService.CreateRoleAsync(input, ct);
        return HandleResult(result);
    }

    [HttpGet]
    [HasPermission(AppPermissions.Identity.RolesRead)]
    public async Task<IActionResult> ListRoles(CancellationToken ct)
    {
        var result = await roleService.ListRolesAsync(ct);
        return HandleResult(result);
    }

    [HttpGet(ApiRoutes.Roles.ById)]
    [HasPermission(AppPermissions.Identity.RolesRead)]
    public async Task<IActionResult> GetRole(Guid id, CancellationToken ct)
    {
        var result = await roleService.GetByIdAsync(id, ct);
        return HandleResult(result);
    }

    [HttpPut(ApiRoutes.Roles.ById)]
    [HasPermission(AppPermissions.Identity.RolesWrite)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleInput input, CancellationToken ct)
    {
        var result = await roleService.UpdateRoleAsync(id, input, ct);
        return HandleResult(result);
    }

    [HttpDelete(ApiRoutes.Roles.ById)]
    [HasPermission(AppPermissions.Identity.RolesDelete)]
    public async Task<IActionResult> DeleteRole(Guid id, CancellationToken ct)
    {
        var result = await roleService.DeleteRoleAsync(id, ct);
        return HandleResult(result);
    }
}
