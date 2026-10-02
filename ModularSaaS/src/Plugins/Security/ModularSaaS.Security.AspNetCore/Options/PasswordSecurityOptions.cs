using System.ComponentModel.DataAnnotations;

namespace ModularSaaS.Security.AspNetCore.Options;

public sealed class PasswordSecurityOptions
{
    [Range(10, 31, ErrorMessage = "BCrypt work factor must be between 10 and 31.")]
    public int WorkFactor { get; set; } = 12;
}
