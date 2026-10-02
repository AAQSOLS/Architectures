namespace ModularSaaS.Observability.Options;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public string ServiceName { get; set; } = "ModularSaaS.Api";

    public string ServiceVersion { get; set; } = "1.0.0";

    public string Exporter { get; set; } = "Console"; // "Otlp", "Console", "None"

    public OtlpOptions Otlp { get; set; } = new();

    public SamplingOptions Sampling { get; set; } = new();

    public RedactionOptions Redaction { get; set; } = new();
}
