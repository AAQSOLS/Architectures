namespace ModularSaaS.Observability.Options;

public sealed class OtlpOptions
{
    public string Endpoint { get; set; } = "http://localhost:4317";

    public string Protocol { get; set; } = "Grpc";
}
