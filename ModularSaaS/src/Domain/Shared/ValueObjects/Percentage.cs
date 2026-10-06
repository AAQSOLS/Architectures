namespace ModularSaaS.Domain.Shared.ValueObjects;

public sealed record Percentage : ValueObject, IComparable<Percentage>
{
    public decimal Value { get; init; }

    private Percentage(decimal value)
    {
        Value = value;
    }

    public static Percentage Create(decimal value)
    {
        if (value is < 0m or > 100m)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Percentage value must be between 0 and 100.");
        }

        return new Percentage(decimal.Round(value, 4, MidpointRounding.AwayFromZero));
    }

    public static Percentage FromFraction(decimal fraction)
    {
        if (fraction is < 0m or > 1m)
        {
            throw new ArgumentOutOfRangeException(nameof(fraction), fraction, "Fraction must be between 0 and 1.");
        }

        return Create(fraction * 100m);
    }

    public static Percentage Zero => new(0m);

    public decimal Factor => Value / 100m;

    public decimal ApplyTo(decimal amount) =>
        decimal.Round(amount * Factor, 4, MidpointRounding.AwayFromZero);

    public int CompareTo(Percentage? other) =>
        other is null ? 1 : Value.CompareTo(other.Value);

    public static Percentage operator +(Percentage left, Percentage right) =>
        Create(left.Value + right.Value);

    public static Percentage operator -(Percentage left, Percentage right) =>
        Create(left.Value - right.Value);

    public static bool operator <(Percentage left, Percentage right) => left.CompareTo(right) < 0;

    public static bool operator <=(Percentage left, Percentage right) => left.CompareTo(right) <= 0;

    public static bool operator >(Percentage left, Percentage right) => left.CompareTo(right) > 0;

    public static bool operator >=(Percentage left, Percentage right) => left.CompareTo(right) >= 0;

    public override string ToString() => $"{Value:0.##}%";
}
