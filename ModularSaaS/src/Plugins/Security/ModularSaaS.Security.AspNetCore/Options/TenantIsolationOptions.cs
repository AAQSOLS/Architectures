namespace ModularSaaS.Security.AspNetCore.Options;

public sealed class TenantIsolationOptions
{
    public bool Enabled { get; set; } = true;

    public bool AllowPlatformAdminBypass { get; set; } = true;
}
