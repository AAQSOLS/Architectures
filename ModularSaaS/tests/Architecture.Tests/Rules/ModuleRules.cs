using Xunit;
using ModularSaaS.Architecture.Tests.Support;
using static ModularSaaS.Architecture.Tests.Support.ArchConfig;

namespace ModularSaaS.Architecture.Tests.Rules;

/// <summary>Module isolation and persistence boundaries.</summary>
public class ModuleRules
{
    private const string Shared = "Shared";

    private static bool IsPublicSurface(string ns) =>
        Ns.HasAny(ns, "Abstractions", "Models", "Permissions", "Constants", "Errors");

    [Fact]
    public void A9a_Application_module_uses_other_modules_only_through_Abstractions_and_Models() =>
        Baseline.Verify("A9a", Deps.Refs(ApplicationAssemblies).Where(r =>
        {
            var from = Ns.ModuleOf(r.From.Namespace, ApplicationNs);
            var to = Ns.ModuleOf(r.ToNamespace, ApplicationNs);
            return from is not null && to is not null && from != to && from != Shared && to != Shared &&
                   !IsPublicSurface(r.ToNamespace);
        }));

    [Fact]
    public void A9b_Application_module_does_not_use_other_modules_Domain_types() =>
        Baseline.Verify("A9b", Deps.Refs(ApplicationAssemblies).Where(r =>
        {
            var from = Ns.ModuleOf(r.From.Namespace, ApplicationNs);
            var to = Ns.ModuleOf(r.ToNamespace, DomainNs);
            return from is not null && to is not null && from != to && from != Shared && to != Shared &&
                   !Ns.Has(r.ToNamespace, "Enums");
        }));

    [Fact]
    public void A10_Domain_module_references_other_modules_by_id_only() =>
        Baseline.Verify("A10", Deps.Refs(DomainAssemblies).Where(r =>
        {
            var from = Ns.ModuleOf(r.From.Namespace, DomainNs);
            var to = Ns.ModuleOf(r.ToNamespace, DomainNs);
            return from is not null && to is not null && from != to && from != Shared && to != Shared &&
                   !Ns.Has(r.ToNamespace, "Enums");
        }));

    [Fact]
    public void A14_Readers_do_not_use_repositories() =>
        Baseline.Verify("A14", Deps.Refs(InfrastructureAssemblies).Where(r =>
            Ns.Under(r.From.Namespace, InfrastructureNs) && Ns.Has(r.From.Namespace, "Readers") &&
            r.To.Name.Contains("Repository", StringComparison.Ordinal)));
}
