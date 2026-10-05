using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using ModularSaaS.Security.Constants;

namespace ModularSaaS.Security.Authorization;

public sealed class PermissionAuthorizationHandler(IServiceProvider serviceProvider) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // 1. Direct permission claim on the principal
        if (context.User.HasClaim(c => c.Type == SecurityClaimTypes.Permission &&
                                       string.Equals(c.Value, requirement.Permission, StringComparison.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
            return;
        }

        // 2. Platform super admins bypass tenant permission requirements
        if (context.User.IsInRole(SecurityRoles.PlatformAdmin) ||
            context.User.HasClaim(c => (c.Type == SecurityClaimTypes.Role || c.Type == ClaimTypes.Role) &&
                                       string.Equals(c.Value, SecurityRoles.PlatformAdmin, StringComparison.OrdinalIgnoreCase)) ||
            context.User.HasClaim(c => c.Type == SecurityClaimTypes.Scope &&
                                       string.Equals(c.Value, "Platform", StringComparison.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
            return;
        }

        // 3. Fallback to IPermissionEvaluator if registered by consumer
        var evaluator = serviceProvider.GetService<IPermissionEvaluator>();
        if (evaluator is not null)
        {
            var hasPermission = await evaluator.HasPermissionAsync(requirement.Permission);
            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }
    }
}
