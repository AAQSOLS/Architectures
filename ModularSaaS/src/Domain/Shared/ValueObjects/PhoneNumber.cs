using System.Text.RegularExpressions;

namespace ModularSaaS.Domain.Shared.ValueObjects;

public sealed partial record PhoneNumber : ValueObject
{
    [GeneratedRegex(@"^\+?[0-9\s\-\(\)\.]{7,25}$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"\D", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex NonDigitRegex();

    public string Value { get; init; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var trimmed = value.Trim();
        if (!PhoneRegex().IsMatch(trimmed))
        {
            throw new ArgumentException($"'{value}' is not a valid phone number format.", nameof(value));
        }

        var digitsOnly = NonDigitRegex().Replace(trimmed, string.Empty);
        if (digitsOnly.Length is < 7 or > 15)
        {
            throw new ArgumentException($"Phone number must contain between 7 and 15 digits.", nameof(value));
        }

        return new PhoneNumber(trimmed);
    }

    public static bool TryCreate(string? value, out PhoneNumber? phoneNumber)
    {
        phoneNumber = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        if (!PhoneRegex().IsMatch(trimmed))
        {
            return false;
        }

        var digitsOnly = NonDigitRegex().Replace(trimmed, string.Empty);
        if (digitsOnly.Length is < 7 or > 15)
        {
            return false;
        }

        phoneNumber = new PhoneNumber(trimmed);
        return true;
    }

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;

    public override string ToString() => Value;
}
