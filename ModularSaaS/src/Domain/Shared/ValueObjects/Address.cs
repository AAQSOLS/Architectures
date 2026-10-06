namespace ModularSaaS.Domain.Shared.ValueObjects;

public sealed record Address : ValueObject
{
    public string Street { get; init; }

    public string? UnitOrSuite { get; init; }

    public string City { get; init; }

    public string StateOrProvince { get; init; }

    public string PostalCode { get; init; }

    public string Country { get; init; }

    private Address(
        string street,
        string? unitOrSuite,
        string city,
        string stateOrProvince,
        string postalCode,
        string country)
    {
        Street = street;
        UnitOrSuite = unitOrSuite;
        City = city;
        StateOrProvince = stateOrProvince;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Create(
        string street,
        string city,
        string stateOrProvince,
        string postalCode,
        string country,
        string? unitOrSuite = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(street);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(stateOrProvince);
        ArgumentException.ThrowIfNullOrWhiteSpace(postalCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(country);

        return new Address(
            street.Trim(),
            string.IsNullOrWhiteSpace(unitOrSuite) ? null : unitOrSuite.Trim(),
            city.Trim(),
            stateOrProvince.Trim(),
            postalCode.Trim(),
            country.Trim().ToUpperInvariant());
    }

    public override string ToString()
    {
        var unitPart = string.IsNullOrWhiteSpace(UnitOrSuite) ? string.Empty : $" {UnitOrSuite},";
        return $"{Street},{unitPart} {City}, {StateOrProvince} {PostalCode}, {Country}".Trim();
    }
}
