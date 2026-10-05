using System.Diagnostics.Metrics;

namespace ModularSaaS.Observability.Diagnostics;

public static class SaaSMetrics
{
    public const string MeterName = "ModularSaaS";

    public static readonly Meter Meter = new(MeterName, "1.0.0");

    private static readonly Counter<long> RequestsCounter = Meter.CreateCounter<long>(
        "saas_requests_total",
        description: "Total number of HTTP requests processed by tenant.");

    private static readonly Counter<long> LoginCounter = Meter.CreateCounter<long>(
        "saas_auth_login_total",
        description: "Total login attempts tagged by result.");

    private static readonly Counter<long> TokenRevocationCounter = Meter.CreateCounter<long>(
        "saas_token_revocations_total",
        description: "Total number of security token revocations.");

    private static readonly Counter<long> TenantCreationCounter = Meter.CreateCounter<long>(
        "saas_tenants_created_total",
        description: "Total new tenants provisioned.");

    public static void RecordRequest(string? tenantId, int statusCode, string? route)
    {
        RequestsCounter.Add(1,
            new KeyValuePair<string, object?>("tenant.id", tenantId ?? "none"),
            new KeyValuePair<string, object?>("http.status_code", statusCode),
            new KeyValuePair<string, object?>("http.route", route ?? "unknown"));
    }

    public static void RecordLogin(bool isSuccess, string? failureReason = null)
    {
        LoginCounter.Add(1,
            new KeyValuePair<string, object?>("result", isSuccess ? "success" : "failure"),
            new KeyValuePair<string, object?>("reason", failureReason ?? "none"));
    }

    public static void RecordTokenRevocation()
    {
        TokenRevocationCounter.Add(1);
    }

    public static void RecordTenantCreated()
    {
        TenantCreationCounter.Add(1);
    }
}
