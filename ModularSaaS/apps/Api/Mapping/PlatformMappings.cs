using Mapster;
using ModularSaaS.Api.Contracts.Platform;
using ModularSaaS.Application.Platform.Models;

namespace ModularSaaS.Api.Mapping;

/// <summary>
/// Host-layer mappings between API contracts and Application Platform models.
/// </summary>
internal sealed class PlatformMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Request -> Input
        config.NewConfig<PlatformLoginRequest, PlatformLoginInput>();
        config.NewConfig<ImpersonateRequest, ImpersonateInput>();

        // Result -> Response
        config.NewConfig<PlatformAuthResult, PlatformAuthResponse>();
        config.NewConfig<ImpersonationResult, ImpersonationResponse>();
    }
}
