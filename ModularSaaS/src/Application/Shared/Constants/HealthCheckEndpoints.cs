namespace ModularSaaS.Application.Shared.Constants;

public static class HealthCheckEndpoints
{
    public const string Live = "/health/live";
    public const string Ready = "/health/ready";
    public const string Standard = "/health";
    public const string Details = "/health/details";
}
