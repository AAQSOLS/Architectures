namespace ModularSaaS.Architecture.Tests.Support;

internal static class Ns
{
    public static string[] Segments(string ns) => ns.Split('.', StringSplitOptions.RemoveEmptyEntries);

    public static bool Has(string ns, string segment) => Segments(ns).Contains(segment, StringComparer.Ordinal);

    public static bool HasAny(string ns, params string[] segments) => segments.Any(s => Has(ns, s));

    public static bool Under(string ns, string prefix) =>
        ns.Equals(prefix, StringComparison.Ordinal) || ns.StartsWith(prefix + ".", StringComparison.Ordinal);

    /// <summary>Module = first namespace segment after the layer prefix ("Root.Application.Sales.Models" -> "Sales").</summary>
    public static string? ModuleOf(string ns, string layerPrefix)
    {
        if (!ns.StartsWith(layerPrefix + ".", StringComparison.Ordinal))
        {
            return null;
        }

        return Segments(ns[(layerPrefix.Length + 1)..]).FirstOrDefault();
    }
}
