using Mapster;

namespace ModularSaaS.Application.Platform.Mapping;

/// <summary>
/// Entity-to-result mappings for the Platform module.
/// Platform results are token-based (PlatformAuthResult, ImpersonationResult)
/// built from service logic, not entity projection. No entity mappings needed.
/// </summary>
internal sealed class PlatformMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Intentionally empty: Platform results are composed in service logic,
        // not mapped from entities.
    }
}
