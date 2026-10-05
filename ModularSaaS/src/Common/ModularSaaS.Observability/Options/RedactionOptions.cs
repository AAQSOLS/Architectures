namespace ModularSaaS.Observability.Options;

public sealed class RedactionOptions
{
    public bool MaskQueryStrings { get; set; } = true;

    public List<string> RedactedHeaders { get; set; } = ["Authorization", "Cookie", "Set-Cookie", "X-Api-Key"];
}
