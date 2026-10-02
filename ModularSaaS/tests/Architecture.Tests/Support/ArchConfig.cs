namespace ModularSaaS.Architecture.Tests.Support;

internal static class ArchConfig
{
    /// <summary>Root namespace of the TARGET structure.</summary>
    public const string Root = "ModularSaaS";

    // Assembly (project) names of the solution.
    public static readonly string[] DomainAssemblies = ["ModularSaaS.Domain"];
    public static readonly string[] ApplicationAssemblies = ["ModularSaaS.Application"];
    public static readonly string[] InfrastructureAssemblies = ["ModularSaaS.Infrastructure"];
    public static readonly string[] HostAssemblies = ["ModularSaaS.Api"];

    // Target namespace prefixes
    public static string DomainNs => Root + ".Domain";
    public static string ApplicationNs => Root + ".Application";
    public static string InfrastructureNs => Root + ".Infrastructure";

    public static bool In(string assembly, params IEnumerable<string>[] groups) =>
        groups.Any(g => g.Contains(assembly, StringComparer.Ordinal));
}
