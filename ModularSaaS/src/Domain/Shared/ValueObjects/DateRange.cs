namespace ModularSaaS.Domain.Shared.ValueObjects;

public sealed record DateRange : ValueObject
{
    public DateTimeOffset StartUtc { get; init; }

    public DateTimeOffset? EndUtc { get; init; }

    private DateRange(DateTimeOffset startUtc, DateTimeOffset? endUtc)
    {
        StartUtc = startUtc;
        EndUtc = endUtc;
    }

    public static DateRange Create(DateTimeOffset startUtc, DateTimeOffset? endUtc = null)
    {
        if (endUtc.HasValue && endUtc.Value < startUtc)
        {
            throw new ArgumentException($"End date '{endUtc.Value}' cannot be earlier than start date '{startUtc}'.", nameof(endUtc));
        }

        return new DateRange(startUtc, endUtc);
    }

    public bool IsOpenEnded => !EndUtc.HasValue;

    public TimeSpan? Duration => EndUtc.HasValue ? EndUtc.Value - StartUtc : null;

    public bool Contains(DateTimeOffset timestamp) =>
        timestamp >= StartUtc && (!EndUtc.HasValue || timestamp <= EndUtc.Value);

    public bool Overlaps(DateRange other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var thisEnd = EndUtc ?? DateTimeOffset.MaxValue;
        var otherEnd = other.EndUtc ?? DateTimeOffset.MaxValue;

        return StartUtc < otherEnd && other.StartUtc < thisEnd;
    }

    public override string ToString() =>
        EndUtc.HasValue ? $"{StartUtc:u} - {EndUtc:u}" : $"{StartUtc:u} - Ongoing";
}
