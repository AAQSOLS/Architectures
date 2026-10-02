using Asp.Versioning;
using Mapster;
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
[Route(ApiRoutes.Roles.Prefix)]
public sealed class RolesController(IRoleService roleService) : BaseApiController
{
    [HttpPost]
    [HasPermission(AppPermissions.Identity.RolesWrite)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        var input = request.Adapt<CreateRoleInput>();
        var result = await roleService.CreateRoleAsync(input, ct);
        return HandleResult<RoleResult, RoleResponse>(result);
    }

    [HttpGet]
    [HasPermission(AppPermissions.Identity.RolesRead)]
    public async Task<IActionResult> ListRoles(CancellationToken ct)
    {
        var result = await roleService.ListRolesAsync(ct);
        return HandleResult<IReadOnlyList<RoleResult>, IReadOnlyList<RoleResponse>>(result);
    }

    [HttpGet(ApiRoutes.Roles.ById)]
    [HasPermission(AppPermissions.Identity.RolesRead)]
    public async Task<IActionResult> GetRole(Guid id, CancellationToken ct)
    {
        var result = await roleService.GetByIdAsync(id, ct);
        return HandleResult<RoleDetailsResult, RoleDetailsResponse>(result);
    }

    [HttpPut(ApiRoutes.Roles.ById)]
    [HasPermission(AppPermissions.Identity.RolesWrite)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken ct)
    {
        var input = request.Adapt<UpdateRoleInput>();
        var result = await roleService.UpdateRoleAsync(id, input, ct);
        return HandleResult<RoleResult, RoleResponse>(result);
    }

    [HttpDelete(ApiRoutes.Roles.ById)]
    [HasPermission(AppPermissions.Identity.RolesDelete)]
    public async Task<IActionResult> DeleteRole(Guid id, CancellationToken ct)
    {
        var result = await roleService.DeleteRoleAsync(id, ct);
        return HandleResult(result);
    }
}

