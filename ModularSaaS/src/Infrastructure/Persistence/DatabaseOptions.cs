using System.ComponentModel.DataAnnotations;

namespace ModularSaaS.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";
    public const string DefaultConnectionName = "DefaultConnection";

    [Required(ErrorMessage = "Connection string 'DefaultConnection' is required.")]
    public string DefaultConnection { get; set; } = string.Empty;
}
