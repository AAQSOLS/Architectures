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
using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Api.Controllers.Identity;

[ApiVersion(ApiVersionConstants.V1String)]
[Authorize]
[Route(ApiRoutes.Users.Prefix)]
public sealed class UsersController(IUserService userService) : BaseApiController
{
    [HttpGet(ApiRoutes.Users.Me)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var result = await userService.GetCurrentUserAsync(ct);
        return HandleResult<UserDetailsResult, UserDetailsResponse>(result);
    }

    [HttpPost(ApiRoutes.Users.Register)]
    [HasPermission(AppPermissions.Identity.UsersWrite)]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request, CancellationToken ct)
    {
        var input = request.Adapt<RegisterUserInput>();
        var result = await userService.RegisterUserAsync(input, ct);
        return HandleResult<UserResult, UserResponse>(result);
    }

    [HttpGet]
    [HasPermission(AppPermissions.Identity.UsersRead)]
    public async Task<IActionResult> ListUsers(
        [FromQuery] int pageNumber = PaginationConstants.DefaultPageNumber,
        [FromQuery] int pageSize = PaginationConstants.DefaultPageSize,
        CancellationToken ct = default)
    {
        var page = new PageRequest(pageNumber, pageSize);
        var result = await userService.ListUsersAsync(page, ct);
        return HandleResult<PagedResult<UserListItem>, PagedResult<UserListItemResponse>>(result);
    }

    [HttpPost(ApiRoutes.Users.ChangePassword)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var input = request.Adapt<ChangePasswordInput>();
        var result = await userService.ChangePasswordAsync(input, ct);
        return HandleResult(result);
    }

    [HttpPost(ApiRoutes.Users.AssignRole)]
    [HasPermission(AppPermissions.Identity.RolesWrite)]
    public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleRequest request, CancellationToken ct)
    {
        var result = await userService.AssignRoleAsync(id, request.RoleId, request.ExpiresAtUtc, ct);
        return HandleResult(result);
    }

    [HttpDelete(ApiRoutes.Users.RemoveRole)]
    [HasPermission(AppPermissions.Identity.RolesWrite)]
    public async Task<IActionResult> RemoveRole(Guid id, Guid roleId, CancellationToken ct)
    {
        var result = await userService.RemoveRoleAsync(id, roleId, ct);
        return HandleResult(result);
    }
}

