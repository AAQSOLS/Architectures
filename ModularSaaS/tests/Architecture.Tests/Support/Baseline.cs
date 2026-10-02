using Xunit;

namespace ModularSaaS.Architecture.Tests.Support;

/// <summary>
/// Ratchet: violations are recorded in Baseline/{RuleId}.txt.
/// - A violation NOT in the baseline fails the test.
/// - A baseline entry that no longer violates ALSO fails (ratchet downward).
/// </summary>
internal static class Baseline
{
    public static void Verify(string ruleId, IEnumerable<Ref> refs) =>
        Verify(ruleId, refs.Select(r => r.ToString()));

    public static void Verify(string ruleId, IEnumerable<string> violations)
    {
        var current = violations.Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList();
        var currentSet = current.ToHashSet(StringComparer.Ordinal);
        var file = Path.Combine(RepoPaths.ProjectDir(), "Baseline", ruleId + ".txt");

        if (Environment.GetEnvironmentVariable("ARCH_BASELINE_UPDATE") == "1")
        {
            if (Environment.GetEnvironmentVariable("CI") is not null)
            {
                throw new InvalidOperationException("Baseline generation is not allowed in CI.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllLines(file, current);
            return;
        }

        var baseline = File.Exists(file)
            ? File.ReadAllLines(file).Where(l => l.Length > 0 && !l.StartsWith('#')).ToHashSet(StringComparer.Ordinal)
            : [];

        var added = current.Where(c => !baseline.Contains(c)).ToList();
        var stale = baseline.Where(b => !currentSet.Contains(b)).OrderBy(x => x, StringComparer.Ordinal).ToList();

        Assert.True(added.Count == 0,
            $"[{ruleId}] {added.Count} NEW violation(s). Fix them; do NOT add them to the baseline:\n  " +
            string.Join("\n  ", added.Take(50)));
        Assert.True(stale.Count == 0,
            $"[{ruleId}] {stale.Count} baseline entr(ies) no longer violate. Delete these lines from Baseline/{ruleId}.txt:\n  " +
            string.Join("\n  ", stale.Take(50)));
    }
}
