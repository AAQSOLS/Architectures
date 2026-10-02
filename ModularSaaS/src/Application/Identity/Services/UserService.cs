using FluentValidation;
using Mapster;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Identity.Errors;
using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Application.Shared.Models;
using ModularSaaS.Domain.Identity;

namespace ModularSaaS.Application.Identity.Services;

internal sealed class UserService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IIdentityReader identityReader,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    ICurrentUser currentUser,
    IPermissionCacheInvalidator permissionCacheInvalidator,
    IValidator<RegisterUserInput> registerValidator,
    IValidator<ChangePasswordInput> changePasswordValidator) : IUserService
{
    public async Task<Result<UserResult>> RegisterUserAsync(RegisterUserInput input, CancellationToken ct = default)
    {
        var validation = await registerValidator.ValidateAsync(input, ct);
        if (!validation.IsValid)
        {
            return Result.Failure<UserResult>(UserErrors.Validation(validation.Errors[0].ErrorMessage));
        }

        var tenantId = input.TenantId.HasValue && (tenantContext.IsPlatformScope || currentUser.IsPlatformAdmin)
            ? input.TenantId.Value
            : tenantContext.RequireTenantId();
        if (await userRepository.EmailExistsAsync(tenantId, input.Email, ct))
        {
            return Result.Failure<UserResult>(UserErrors.EmailExists);
        }

        var passwordHash = passwordHasher.Hash(input.Password);
        var user = new User(tenantId, input.Email, passwordHash, input.FirstName, input.LastName);

        await userRepository.AddAsync(user, ct);

        // Assign specified role or default tenant role
        var roleId = input.RoleId;
        if (!roleId.HasValue)
        {
            var defaultRole = await roleRepository.GetDefaultRoleAsync(tenantId, ct);
            if (defaultRole is not null)
            {
                roleId = defaultRole.Id;
            }
        }

        if (roleId.HasValue)
        {
            await userRepository.AssignRoleAsync(user.Id, roleId.Value, currentUser.UserId, ct: ct);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return user.Adapt<UserResult>();
    }

    public async Task<Result<UserDetailsResult>> GetCurrentUserAsync(CancellationToken ct = default)
    {
        if (!currentUser.UserId.HasValue)
        {
            return Result.Failure<UserDetailsResult>(UserErrors.NotFound);
        }

        return await GetByIdAsync(currentUser.UserId.Value, ct);
    }

    public async Task<Result<UserDetailsResult>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(id, ct);
        if (user is null)
        {
            return Result.Failure<UserDetailsResult>(UserErrors.NotFound);
        }

        var roles = await userRepository.GetUserRoleNamesAsync(user.Id, ct);
        var permissions = await userRepository.GetEffectivePermissionsAsync(user.TenantId, user.Id, ct);

        return new UserDetailsResult(
            user.Id,
            user.TenantId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Status,
            user.EmailConfirmed,
            roles,
            permissions.ToList(),
            user.LastLoginAtUtc,
            user.CreatedAtUtc);
    }

    public async Task<Result<PagedResult<UserListItem>>> ListUsersAsync(PageRequest page, CancellationToken ct = default)
    {
        var tenantId = tenantContext.RequireTenantId();
        var result = await identityReader.ListUsersAsync(tenantId, page, ct);
        return result;
    }

    public async Task<Result> ChangePasswordAsync(ChangePasswordInput input, CancellationToken ct = default)
    {
        var validation = await changePasswordValidator.ValidateAsync(input, ct);
        if (!validation.IsValid)
        {
            return Result.Failure(UserErrors.Validation(validation.Errors[0].ErrorMessage));
        }

        if (!currentUser.UserId.HasValue)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        var user = await userRepository.GetByIdAsync(currentUser.UserId.Value, ct);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        if (!passwordHasher.Verify(input.CurrentPassword, user.PasswordHash))
        {
            return Result.Failure(AuthErrors.InvalidCredentials);
        }

        user.SetPasswordHash(passwordHasher.Hash(input.NewPassword));
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> AssignRoleAsync(Guid userId, Guid roleId, DateTimeOffset? expiresAtUtc = null, CancellationToken ct = default)
    {
        var tenantId = tenantContext.RequireTenantId();
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user is null || user.TenantId != tenantId)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        var role = await roleRepository.GetByIdAsync(roleId, ct);
        if (role is null || role.TenantId != tenantId)
        {
            return Result.Failure(RoleErrors.NotFound);
        }

        await userRepository.AssignRoleAsync(userId, roleId, currentUser.UserId, expiresAtUtc, ct);
        await unitOfWork.SaveChangesAsync(ct);
        await permissionCacheInvalidator.InvalidateAsync(tenantId, userId, ct);

        return Result.Success();
    }

    public async Task<Result> RemoveRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default)
    {
        var tenantId = tenantContext.RequireTenantId();
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user is null || user.TenantId != tenantId)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        await userRepository.RemoveRoleAsync(userId, roleId, ct);
        await unitOfWork.SaveChangesAsync(ct);
        await permissionCacheInvalidator.InvalidateAsync(tenantId, userId, ct);

        return Result.Success();
    }

    public async Task<Result<UserImpersonationData>> GetImpersonationDataAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var targetUser = await userRepository.GetByIdAsync(userId, ct);
        if (targetUser is null || targetUser.TenantId != tenantId)
        {
            return Result.Failure<UserImpersonationData>(UserErrors.NotFound);
        }

        var roles = await userRepository.GetUserRoleNamesAsync(targetUser.Id, ct);
        var permissions = await userRepository.GetEffectivePermissionsAsync(tenantId, targetUser.Id, ct);

        return new UserImpersonationData(targetUser.Id, targetUser.TenantId, targetUser.Email, roles, permissions);
    }
}
