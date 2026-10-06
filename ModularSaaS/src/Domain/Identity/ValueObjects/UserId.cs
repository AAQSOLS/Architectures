using ModularSaaS.Domain.Shared;

namespace ModularSaaS.Domain.Identity.ValueObjects;

public readonly record struct UserId(Guid Value) : IValueObject, IComparable<UserId>
{
    public static UserId New() => new(Guid.NewGuid());

    public static UserId Empty => new(Guid.Empty);

    public static UserId From(Guid value) => new(value);

    public static implicit operator Guid(UserId id) => id.Value;

    public static implicit operator UserId(Guid value) => new(value);

    public int CompareTo(UserId other) => Value.CompareTo(other.Value);

    public static bool operator <(UserId left, UserId right) => left.CompareTo(right) < 0;

    public static bool operator <=(UserId left, UserId right) => left.CompareTo(right) <= 0;

    public static bool operator >(UserId left, UserId right) => left.CompareTo(right) > 0;

    public static bool operator >=(UserId left, UserId right) => left.CompareTo(right) >= 0;

    public override string ToString() => Value.ToString();
}
