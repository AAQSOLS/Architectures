using System.Text.RegularExpressions;
using Xunit;
using ModularSaaS.Architecture.Tests.Support;
using static ModularSaaS.Architecture.Tests.Support.ArchConfig;

namespace ModularSaaS.Architecture.Tests.Rules;

/// <summary>Hosts see only Application Abstractions + Models, and Domain enums.</summary>
public partial class HostRules
{
    [GeneratedRegex(@"^I\w*Repository(`\d+)?$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex RepositoryName();

    private static bool IsAppSurface(Ref r) =>
        Ns.HasAny(r.ToNamespace, "Abstractions", "Models", "Permissions", "Exceptions", "Constants", "Errors") ||
        (r.To.Name == "DependencyInjection" && r.ToNamespace == ApplicationNs);

    [Fact]
    public void A5b_Hosts_use_only_Application_Abstractions_and_Models() =>
        Baseline.Verify("A5b", Deps.Refs(HostAssemblies).Where(r =>
            In(r.ToAssembly, ApplicationAssemblies) && (!IsAppSurface(r) || RepositoryName().IsMatch(r.To.Name))));

    [Fact]
    public void A6_Hosts_use_Domain_only_through_Enums() =>
        Baseline.Verify("A6", Deps.Refs(HostAssemblies).Where(r =>
            (In(r.ToAssembly, DomainAssemblies) || Ns.Under(r.ToNamespace, DomainNs)) &&
            !Ns.Has(r.ToNamespace, "Enums")));
}
