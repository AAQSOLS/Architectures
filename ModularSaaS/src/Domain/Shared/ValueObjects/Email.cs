using System.Text.RegularExpressions;

namespace ModularSaaS.Domain.Shared.ValueObjects;

public sealed partial record Email : ValueObject
{
    private const int MaxEmailLength = 256;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex EmailRegex();

    public string Value { get; init; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var trimmed = value.Trim();
        if (trimmed.Length > MaxEmailLength)
        {
            throw new ArgumentException($"Email exceeds maximum length of {MaxEmailLength} characters.", nameof(value));
        }

        if (!EmailRegex().IsMatch(trimmed))
        {
            throw new ArgumentException($"'{value}' is not a valid email address.", nameof(value));
        }

        return new Email(trimmed.ToLowerInvariant());
    }

    public static bool TryCreate(string? value, out Email? email)
    {
        email = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > MaxEmailLength || !EmailRegex().IsMatch(trimmed))
        {
            return false;
        }

        email = new Email(trimmed.ToLowerInvariant());
        return true;
    }

    public static implicit operator string(Email email) => email.Value;

    public override string ToString() => Value;
}
