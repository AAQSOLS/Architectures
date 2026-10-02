using System.ComponentModel.DataAnnotations;

namespace ModularSaaS.Infrastructure.Security;

internal sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required(ErrorMessage = "JWT Secret is required.")]
    [MinLength(32, ErrorMessage = "JWT Secret must be at least 32 characters long.")]
    public string Secret { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Issuer is required.")]
    public string Issuer { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Audience is required.")]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 1440, ErrorMessage = "JWT ExpiryMinutes must be between 1 and 1440.")]
    public int ExpiryMinutes { get; set; } = 60;

    [Range(1, 90, ErrorMessage = "JWT RefreshTokenDays must be between 1 and 90.")]
    public int RefreshTokenDays { get; set; } = 7;
}
