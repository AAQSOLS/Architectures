using System.Diagnostics;

namespace ModularSaaS.Observability.Diagnostics;

public static class SaaSActivities
{
    public const string SourceName = "ModularSaaS";

    public static readonly ActivitySource Source = new(SourceName, "1.0.0");

    public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return Source.HasListeners() ? Source.StartActivity(name, kind) : null;
    }
}
