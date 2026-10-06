using System.Text.RegularExpressions;

namespace ModularSaaS.Domain.Shared.ValueObjects;

public sealed partial record Money : ValueObject, IComparable<Money>
{
    [GeneratedRegex(@"^[A-Z]{3}$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex CurrencyCodeRegex();

    public decimal Amount { get; init; }

    public string Currency { get; init; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        var normalizedCurrency = currency.Trim().ToUpperInvariant();
        if (!CurrencyCodeRegex().IsMatch(normalizedCurrency))
        {
            throw new ArgumentException($"Currency code '{currency}' must be a valid 3-letter ISO code.", nameof(currency));
        }

        return new Money(decimal.Round(amount, 4, MidpointRounding.AwayFromZero), normalizedCurrency);
    }

    public static Money Zero(string currency = "USD") => Create(0m, currency);

    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor) =>
        new(decimal.Round(Amount * factor, 4, MidpointRounding.AwayFromZero), Currency);

    public Money Allocate(decimal percentage) =>
        Multiply(percentage / 100m);

    public int CompareTo(Money? other)
    {
        if (other is null)
        {
            return 1;
        }

        EnsureSameCurrency(other);
        return Amount.CompareTo(other.Amount);
    }

    public static Money operator +(Money left, Money right) => left.Add(right);

    public static Money operator -(Money left, Money right) => left.Subtract(right);

    public static Money operator *(Money left, decimal factor) => left.Multiply(factor);

    public static Money operator *(decimal factor, Money right) => right.Multiply(factor);

    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;

    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;

    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;

    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;

    public override string ToString() => $"{Amount:0.00} {Currency}";

    private void EnsureSameCurrency(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Cannot operate on money with different currencies: '{Currency}' and '{other.Currency}'.");
        }
    }
}
