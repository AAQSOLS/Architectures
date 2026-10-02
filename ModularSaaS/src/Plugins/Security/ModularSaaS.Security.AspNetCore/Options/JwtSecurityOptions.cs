using System.ComponentModel.DataAnnotations;

namespace ModularSaaS.Security.AspNetCore.Options;

public sealed class JwtSecurityOptions
{
    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT SigningKey is required.")]
    [MinLength(32, ErrorMessage = "JWT SigningKey must be at least 32 characters (256 bits).")]
    public string SigningKey { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 15;

    public TimeSpan ClockSkew { get; set; } = TimeSpan.Zero;

    public bool RequireHttpsMetadata { get; set; }

    public List<string> PreviousValidationKeys { get; set; } = [];
}
