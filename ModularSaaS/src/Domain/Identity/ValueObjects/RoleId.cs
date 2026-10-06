using ModularSaaS.Domain.Shared;

namespace ModularSaaS.Domain.Identity.ValueObjects;

public readonly record struct RoleId(Guid Value) : IValueObject, IComparable<RoleId>
{
    public static RoleId New() => new(Guid.NewGuid());

    public static RoleId Empty => new(Guid.Empty);

    public static RoleId From(Guid value) => new(value);

    public static implicit operator Guid(RoleId id) => id.Value;

    public static implicit operator RoleId(Guid value) => new(value);

    public int CompareTo(RoleId other) => Value.CompareTo(other.Value);

    public static bool operator <(RoleId left, RoleId right) => left.CompareTo(right) < 0;

    public static bool operator <=(RoleId left, RoleId right) => left.CompareTo(right) <= 0;

    public static bool operator >(RoleId left, RoleId right) => left.CompareTo(right) > 0;

    public static bool operator >=(RoleId left, RoleId right) => left.CompareTo(right) >= 0;

    public override string ToString() => Value.ToString();
}
