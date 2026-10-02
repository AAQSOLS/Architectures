using Xunit;
using ModularSaaS.Architecture.Tests.Support;
using static ModularSaaS.Architecture.Tests.Support.ArchConfig;

namespace ModularSaaS.Architecture.Tests.Rules;

/// <summary>Layer and host boundaries (assembly based).</summary>
public class LayerRules
{
    [Fact]
    public void A1_Domain_depends_on_nothing_outward() =>
        Baseline.Verify("A1", Deps.Refs(DomainAssemblies).Where(r =>
            In(r.ToAssembly, ApplicationAssemblies, InfrastructureAssemblies, HostAssemblies) ||
            Ns.Under(r.ToNamespace, "Microsoft.EntityFrameworkCore") ||
            Ns.Under(r.ToNamespace, "Mapster")));

    [Fact]
    public void A2_Application_does_not_depend_on_Infrastructure_or_hosts() =>
        Baseline.Verify("A2", Deps.Refs(ApplicationAssemblies).Where(r => In(r.ToAssembly, InfrastructureAssemblies, HostAssemblies)));

    [Fact]
    public void A3_Infrastructure_does_not_depend_on_hosts() =>
        Baseline.Verify("A3", Deps.Refs(InfrastructureAssemblies).Where(r => In(r.ToAssembly, HostAssemblies)));

    [Fact]
    public void A5_Hosts_use_Infrastructure_only_from_Program() =>
        Baseline.Verify("A5", Deps.Refs(HostAssemblies).Where(r => In(r.ToAssembly, InfrastructureAssemblies) && !r.FromIsProgram));

    [Fact]
    public void A7_Hosts_do_not_use_EntityFramework() =>
        Baseline.Verify("A7", Deps.Refs(HostAssemblies).Where(r =>
            Ns.Under(r.ToNamespace, "Microsoft.EntityFrameworkCore") && !r.FromIsProgram));
}
