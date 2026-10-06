using System.Text.RegularExpressions;
using Mono.Cecil;
using Xunit;
using ModularSaaS.Architecture.Tests.Support;
using static ModularSaaS.Architecture.Tests.Support.ArchConfig;

namespace ModularSaaS.Architecture.Tests.Rules;

/// <summary>Shape of Application/host public surface and Domain enums.</summary>
public partial class SurfaceRules
{
    [GeneratedRegex(@"^I\w+(Service|Reader)$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ServiceOrReaderInterface();

    private static bool IsDomainNonEnum(TypeReference t) =>
        (In(Deps.AssemblyName(t), DomainAssemblies) || Ns.Under(t.Namespace, DomainNs)) && !Ns.Has(t.Namespace, "Enums");

    [Fact]
    public void A8_Application_public_surface_exposes_no_Domain_types_except_enums()
    {
        var surface = Deps.Types(ApplicationAssemblies).Where(t => t.IsPublic &&
            (Ns.Has(t.Namespace, "Models") ||
             (Ns.Has(t.Namespace, "Abstractions") && t.IsInterface && ServiceOrReaderInterface().IsMatch(t.Name))));

        Baseline.Verify("A8", surface.SelectMany(t =>
            Deps.SignatureRefs(t).Where(IsDomainNonEnum).Select(d => $"{t.FullName} -> {d.FullName}")));
    }

    [Fact]
    public void A11_Services_Validators_Mapping_are_internal() =>
        Baseline.Verify("A11", Deps.Types(ApplicationAssemblies)
            .Where(t => t.IsPublic && Ns.HasAny(t.Namespace, "Services", "Validators", "Mapping"))
            .Select(t => $"{t.FullName} (public, must be internal)"));

    [Fact]
    public void A12_Models_Contracts_ViewModels_are_sealed()
    {
        static bool NeedsSeal(TypeDefinition t) => !t.IsEnum && !t.IsInterface && !(t.IsAbstract && t.IsSealed);

        var models = Deps.Types(ApplicationAssemblies).Where(t => Ns.Has(t.Namespace, "Models"));
        var contracts = Deps.Types(HostAssemblies).Where(t => Ns.Has(t.Namespace, "Contracts"));

        Baseline.Verify("A12", models.Concat(contracts)
            .Where(NeedsSeal)
            .Where(t => !t.IsSealed || t.IsAbstract)
            .Select(t => $"{t.FullName} (must be sealed and non-abstract)"));
    }

    [Fact]
    public void A13_Domain_enums_live_in_Enums_namespace() =>
        Baseline.Verify("A13", Deps.Types(DomainAssemblies)
            .Where(t => t.IsEnum != Ns.Has(t.Namespace, "Enums"))
            .Select(t => t.IsEnum
                ? $"{t.FullName} (enum outside an Enums namespace)"
                : $"{t.FullName} (non-enum inside an Enums namespace)"));
}
