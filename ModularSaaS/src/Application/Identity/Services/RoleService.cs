using FluentValidation;
using Mapster;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Identity.Errors;
using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Application.Shared.Models;
using ModularSaaS.Domain.Identity;

namespace ModularSaaS.Application.Identity.Services;

internal sealed class RoleService(
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    IPermissionCacheInvalidator permissionCacheInvalidator,
    IValidator<CreateRoleInput> createRoleValidator) : IRoleService
{
    public async Task<Result<RoleResult>> CreateRoleAsync(CreateRoleInput input, CancellationToken ct = default)
    {
        var validation = await createRoleValidator.ValidateAsync(input, ct);
        if (!validation.IsValid)
        {
            return Result.Failure<RoleResult>(RoleErrors.Validation(validation.Errors[0].ErrorMessage));
        }

        var tenantId = tenantContext.RequireTenantId();
        var existing = await roleRepository.GetByNameAsync(tenantId, input.Name, ct);
        if (existing is not null)
        {
            return Result.Failure<RoleResult>(RoleErrors.NameExists);
        }

        var role = new Role(tenantId, input.Name, input.Description);
        await roleRepository.AddAsync(role, ct);

        if (input.Permissions.Count > 0)
        {
            var permissions = await permissionRepository.GetByCodesAsync(input.Permissions, ct);
            await roleRepository.SetRolePermissionsAsync(role.Id, permissions.Select(p => p.Id), ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return role.Adapt<RoleResult>();
    }

    public async Task<Result<RoleDetailsResult>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = tenantContext.RequireTenantId();
        var role = await roleRepository.GetByIdAsync(id, ct);
        if (role is null || role.TenantId != tenantId)
        {
            return Result.Failure<RoleDetailsResult>(RoleErrors.NotFound);
        }

        var permissions = await roleRepository.GetRolePermissionCodesAsync(role.Id, ct);
        return new RoleDetailsResult(role.Id, role.Name, role.Description, role.IsSystem, role.IsDefault, permissions);
    }

    public async Task<Result<IReadOnlyList<RoleResult>>> ListRolesAsync(CancellationToken ct = default)
    {
        var tenantId = tenantContext.RequireTenantId();
        var roles = await roleRepository.ListAsync(tenantId, ct);
        var results = roles.Select(r => r.Adapt<RoleResult>()).ToList();
        return results;
    }

    public async Task<Result<RoleResult>> UpdateRoleAsync(Guid id, UpdateRoleInput input, CancellationToken ct = default)
    {
        var tenantId = tenantContext.RequireTenantId();
        var role = await roleRepository.GetByIdAsync(id, ct);
        if (role is null || role.TenantId != tenantId)
        {
            return Result.Failure<RoleResult>(RoleErrors.NotFound);
        }

        if (role.IsSystem)
        {
            return Result.Failure<RoleResult>(RoleErrors.SystemProtected);
        }

        role.UpdateDetails(input.Name, input.Description);

        if (input.Permissions is not null)
        {
            var permissions = await permissionRepository.GetByCodesAsync(input.Permissions, ct);
            await roleRepository.SetRolePermissionsAsync(role.Id, permissions.Select(p => p.Id), ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
        await permissionCacheInvalidator.InvalidateRoleAsync(tenantId, role.Id, ct);

        return role.Adapt<RoleResult>();
    }

    public async Task<Result> DeleteRoleAsync(Guid id, CancellationToken ct = default)
    {
        var tenantId = tenantContext.RequireTenantId();
        var role = await roleRepository.GetByIdAsync(id, ct);
        if (role is null || role.TenantId != tenantId)
        {
            return Result.Failure(RoleErrors.NotFound);
        }

        if (role.IsSystem)
        {
            return Result.Failure(RoleErrors.SystemProtected);
        }

        roleRepository.Remove(role);
        await unitOfWork.SaveChangesAsync(ct);
        await permissionCacheInvalidator.InvalidateRoleAsync(tenantId, role.Id, ct);

        return Result.Success();
    }
}
