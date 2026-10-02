using System.ComponentModel.DataAnnotations;

namespace ModularSaaS.Security.AspNetCore.Options;

public sealed class SecurityOptions
{
    public const string SectionName = "Security";

    [Required]
    public JwtSecurityOptions Jwt { get; set; } = new();

    public PasswordSecurityOptions Password { get; set; } = new();

    public TenantIsolationOptions TenantIsolation { get; set; } = new();
}
