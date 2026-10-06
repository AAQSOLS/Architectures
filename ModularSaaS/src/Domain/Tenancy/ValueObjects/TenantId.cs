using ModularSaaS.Domain.Shared;

namespace ModularSaaS.Domain.Tenancy.ValueObjects;

public readonly record struct TenantId(Guid Value) : IValueObject, IComparable<TenantId>
{
    public static TenantId New() => new(Guid.NewGuid());

    public static TenantId Empty => new(Guid.Empty);

    public static TenantId From(Guid value) => new(value);

    public static implicit operator Guid(TenantId id) => id.Value;

    public static implicit operator TenantId(Guid value) => new(value);

    public int CompareTo(TenantId other) => Value.CompareTo(other.Value);

    public static bool operator <(TenantId left, TenantId right) => left.CompareTo(right) < 0;

    public static bool operator <=(TenantId left, TenantId right) => left.CompareTo(right) <= 0;

    public static bool operator >(TenantId left, TenantId right) => left.CompareTo(right) > 0;

    public static bool operator >=(TenantId left, TenantId right) => left.CompareTo(right) >= 0;

    public override string ToString() => Value.ToString();
}
