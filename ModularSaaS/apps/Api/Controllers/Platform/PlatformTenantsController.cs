using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModularSaaS.Api.Common;
using ModularSaaS.Api.Contracts.Tenancy;
using ModularSaaS.Application.Shared.Constants;
using ModularSaaS.Application.Shared.Models;
using ModularSaaS.Application.Tenancy.Abstractions;
using ModularSaaS.Application.Tenancy.Models;
using ModularSaaS.Domain.Tenancy.Enums;

namespace ModularSaaS.Api.Controllers.Platform;

[ApiVersion(ApiVersionConstants.V1String)]
[Authorize(Roles = AppRoles.PlatformAdmin)]
[Route(ApiRoutes.PlatformTenants.Prefix)]
public sealed class PlatformTenantsController(ITenantService tenantService) : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request, CancellationToken ct)
    {
        var input = request.Adapt<CreateTenantInput>();
        var result = await tenantService.CreateTenantAsync(input, ct);
        return HandleResult<TenantResult, TenantResponse>(result);
    }

    [HttpGet]
    public async Task<IActionResult> ListTenants(
        [FromQuery] TenantStatus? status = null,
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = PaginationConstants.DefaultPageNumber,
        [FromQuery] int pageSize = PaginationConstants.DefaultPageSize,
        CancellationToken ct = default)
    {
        var filter = new TenantFilter(status, search, new PageRequest(pageNumber, pageSize));
        var result = await tenantService.ListTenantsAsync(filter, ct);
        return HandleResult<PagedResult<TenantListItem>, PagedResult<TenantResponse>>(result);
    }

    [HttpGet(ApiRoutes.PlatformTenants.ById)]
    public async Task<IActionResult> GetTenant(Guid id, CancellationToken ct)
    {
        var result = await tenantService.GetByIdAsync(id, ct);
        return HandleResult<TenantResult, TenantResponse>(result);
    }

    [HttpPost(ApiRoutes.PlatformTenants.Suspend)]
    public async Task<IActionResult> SuspendTenant(Guid id, CancellationToken ct)
    {
        var result = await tenantService.SuspendTenantAsync(id, ct);
        return HandleResult(result);
    }

    [HttpPost(ApiRoutes.PlatformTenants.Activate)]
    public async Task<IActionResult> ActivateTenant(Guid id, CancellationToken ct)
    {
        var result = await tenantService.ActivateTenantAsync(id, ct);
        return HandleResult(result);
    }
}

