using Mapster;
using ModularSaaS.Api.Contracts.Identity;
using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Api.Mapping;

/// <summary>
/// Host-layer mappings between API contracts and Application Identity models.
/// </summary>
internal sealed class IdentityMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Request -> Input
        config.NewConfig<LoginRequest, LoginInput>();
        config.NewConfig<RegisterUserRequest, RegisterUserInput>()
            .Ignore(dest => dest.TenantId);
        config.NewConfig<ChangePasswordRequest, ChangePasswordInput>();
        config.NewConfig<ForgotPasswordRequest, ForgotPasswordInput>();
        config.NewConfig<ResetPasswordRequest, ResetPasswordInput>();
        config.NewConfig<RefreshTokenRequest, RefreshTokenInput>();
        config.NewConfig<CreateRoleRequest, CreateRoleInput>();
        config.NewConfig<UpdateRoleRequest, UpdateRoleInput>();

        // Result -> Response
        config.NewConfig<AuthTokensResult, AuthResponse>();
        config.NewConfig<UserResult, UserResponse>();
        config.NewConfig<UserDetailsResult, UserDetailsResponse>();
        config.NewConfig<UserListItem, UserListItemResponse>();
        config.NewConfig<RoleResult, RoleResponse>();
        config.NewConfig<RoleDetailsResult, RoleDetailsResponse>();
        config.NewConfig<PermissionGroupResult, PermissionGroupResponse>();
        config.NewConfig<PermissionItem, PermissionItemResponse>();

        // Paged results
        config.NewConfig<PagedResult<UserListItem>, PagedResult<UserListItemResponse>>();
    }
}
