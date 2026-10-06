namespace ModularSaaS.Observability.Redaction;

public static class SensitiveDataSanitizer
{
    private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Cookie",
        "Set-Cookie",
        "X-Api-Key",
        "Proxy-Authorization"
    };

    public static bool IsSensitiveHeader(string headerName)
    {
        return !string.IsNullOrWhiteSpace(headerName) && SensitiveHeaders.Contains(headerName);
    }

    public static string SanitizeUrl(string rawUrl)
    {
        if (string.IsNullOrWhiteSpace(rawUrl))
        {
            return string.Empty;
        }

        var queryIndex = rawUrl.IndexOf('?', StringComparison.Ordinal);
        if (queryIndex < 0)
        {
            return rawUrl;
        }

        return string.Concat(rawUrl.AsSpan(0, queryIndex), "?REDACTED");
    }
}
