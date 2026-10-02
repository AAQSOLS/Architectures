using System.ComponentModel.DataAnnotations;

namespace ModularSaaS.Infrastructure.Persistence.Seed;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public static readonly Guid DefaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [Required]
    [EmailAddress]
    public string PlatformAdminEmail { get; set; } = "superadmin@modularsaas.com";

    [Required]
    [MinLength(8)]
    public string PlatformAdminPassword { get; set; } = "SuperAdmin123!";

    [Required]
    public string PlatformAdminFirstName { get; set; } = "Platform";

    [Required]
    public string PlatformAdminLastName { get; set; } = "Administrator";

    [Required]
    public string DefaultTenantName { get; set; } = "Default Organization";

    [Required]
    public string DefaultTenantIdentifier { get; set; } = "default";

    [Required]
    [EmailAddress]
    public string DefaultTenantAdminEmail { get; set; } = "admin@default.local";

    [Required]
    [MinLength(8)]
    public string DefaultTenantAdminPassword { get; set; } = "Admin123!";

    [Required]
    public string DefaultTenantAdminFirstName { get; set; } = "Default";

    [Required]
    public string DefaultTenantAdminLastName { get; set; } = "Admin";
}
