namespace ModularSaaS.Domain.Shared.ValueObjects;

public sealed record FullName : ValueObject
{
    public string FirstName { get; init; }

    public string LastName { get; init; }

    private FullName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static FullName Create(string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        return new FullName(firstName.Trim(), lastName.Trim());
    }

    public string DisplayName => $"{FirstName} {LastName}".Trim();

    public override string ToString() => DisplayName;
}
