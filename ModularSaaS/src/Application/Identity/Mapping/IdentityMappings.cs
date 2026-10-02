using Mapster;
using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Domain.Identity;

namespace ModularSaaS.Application.Identity.Mapping;

/// <summary>
/// Entity-to-result mappings for the Identity module.
/// Multi-source results (UserDetailsResult, RoleDetailsResult) stay manual.
/// </summary>
internal sealed class IdentityMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserResult>();
        config.NewConfig<Role, RoleResult>();
    }
}
