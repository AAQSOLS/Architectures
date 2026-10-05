namespace ModularSaaS.Observability.Options;

public sealed class SamplingOptions
{
    public double DefaultRatio { get; set; } = 1.0;

    public bool AlwaysSampleErrors { get; set; } = true;
}
