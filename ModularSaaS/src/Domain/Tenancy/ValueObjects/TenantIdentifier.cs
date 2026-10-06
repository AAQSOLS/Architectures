using System.Text.RegularExpressions;
using ModularSaaS.Domain.Shared;

namespace ModularSaaS.Domain.Tenancy.ValueObjects;

public sealed partial record TenantIdentifier : ValueObject
{
    private const int MinLength = 3;
    private const int MaxLength = 63;

    [GeneratedRegex(@"^[a-z0-9]([a-z0-9-]*[a-z0-9])?$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex IdentifierRegex();

    public string Value { get; init; }

    private TenantIdentifier(string value)
    {
        Value = value;
    }

    public static TenantIdentifier Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length is < MinLength or > MaxLength)
        {
            throw new ArgumentException(
                $"Tenant identifier must be between {MinLength} and {MaxLength} characters long.",
                nameof(value));
        }

        if (!IdentifierRegex().IsMatch(normalized))
        {
            throw new ArgumentException(
                $"Tenant identifier '{value}' is invalid. It must contain only lowercase letters, digits, and hyphens, and cannot start or end with a hyphen.",
                nameof(value));
        }

        return new TenantIdentifier(normalized);
    }

    public static bool TryCreate(string? value, out TenantIdentifier? identifier)
    {
        identifier = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length is < MinLength or > MaxLength || !IdentifierRegex().IsMatch(normalized))
        {
            return false;
        }

        identifier = new TenantIdentifier(normalized);
        return true;
    }

    public static implicit operator string(TenantIdentifier identifier) => identifier.Value;

    public override string ToString() => Value;
}
