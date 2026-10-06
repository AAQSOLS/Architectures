using ModularSaaS.Domain.Identity;
using ModularSaaS.Domain.Identity.ValueObjects;
using ModularSaaS.Domain.Shared.ValueObjects;
using ModularSaaS.Domain.Tenancy;
using ModularSaaS.Domain.Tenancy.ValueObjects;
using Xunit;

namespace ModularSaaS.Architecture.Tests.Rules;

public class ValueObjectTests
{
    [Fact]
    public void Money_CreatesAndPerformsValidArithmetic()
    {
        var m1 = Money.Create(100.50m, "USD");
        var m2 = Money.Create(49.50m, "USD");

        var sum = m1 + m2;
        Assert.Equal(150.00m, sum.Amount);
        Assert.Equal("USD", sum.Currency);

        var diff = m1 - m2;
        Assert.Equal(51.00m, diff.Amount);

        var mult = m1 * 2m;
        Assert.Equal(201.00m, mult.Amount);

        var allocated = m1.Allocate(10m);
        Assert.Equal(10.05m, allocated.Amount);

        var zero = Money.Zero("EUR");
        Assert.Equal(0m, zero.Amount);
        Assert.Equal("EUR", zero.Currency);
    }

    [Fact]
    public void Money_ThrowsOnCurrencyMismatch()
    {
        var usd = Money.Create(100m, "USD");
        var eur = Money.Create(100m, "EUR");

        Assert.Throws<InvalidOperationException>(() => usd + eur);
        Assert.Throws<InvalidOperationException>(() => usd - eur);
        Assert.Throws<InvalidOperationException>(() => usd.CompareTo(eur));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("123")]
    public void Money_ThrowsOnInvalidCurrency(string currency)
    {
        Assert.Throws<ArgumentException>(() => Money.Create(100m, currency));
    }

    [Fact]
    public void Address_CreatesAndFormatsCorrectly()
    {
        var address = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA", "Apt 4B");

        Assert.Equal("123 Main St", address.Street);
        Assert.Equal("Apt 4B", address.UnitOrSuite);
        Assert.Equal("Springfield", address.City);
        Assert.Equal("IL", address.StateOrProvince);
        Assert.Equal("62701", address.PostalCode);
        Assert.Equal("USA", address.Country);
        Assert.Contains("123 Main St, Apt 4B, Springfield, IL 62701, USA", address.ToString());
    }

    [Theory]
    [InlineData("", "City", "State", "12345", "USA")]
    [InlineData("Street", "", "State", "12345", "USA")]
    [InlineData("Street", "City", "", "12345", "USA")]
    [InlineData("Street", "City", "State", "", "USA")]
    [InlineData("Street", "City", "State", "12345", "")]
    public void Address_ThrowsOnMissingRequiredFields(string street, string city, string state, string postal, string country)
    {
        Assert.Throws<ArgumentException>(() => Address.Create(street, city, state, postal, country));
    }

    [Fact]
    public void Email_NormalizesToLowercaseAndValidates()
    {
        var email = Email.Create("  User.Test@Example.COM  ");

        Assert.Equal("user.test@example.com", email.Value);
        Assert.Equal("user.test@example.com", (string)email);

        Assert.True(Email.TryCreate("valid@domain.org", out var parsed));
        Assert.NotNull(parsed);
        Assert.Equal("valid@domain.org", parsed.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("@missinguser.com")]
    [InlineData("missingdomain@")]
    [InlineData("spaces in@domain.com")]
    public void Email_ThrowsOnInvalidFormat(string input)
    {
        Assert.Throws<ArgumentException>(() => Email.Create(input));
        Assert.False(Email.TryCreate(input, out _));
    }

    [Fact]
    public void PhoneNumber_ValidatesAndNormalizes()
    {
        var phone = PhoneNumber.Create("+1 (555) 123-4567");

        Assert.Equal("+1 (555) 123-4567", phone.Value);
        Assert.True(PhoneNumber.TryCreate("+44 20 7946 0958", out var parsed));
        Assert.NotNull(parsed);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("abcdefghij")]
    public void PhoneNumber_ThrowsOnInvalidFormat(string input)
    {
        Assert.Throws<ArgumentException>(() => PhoneNumber.Create(input));
        Assert.False(PhoneNumber.TryCreate(input, out _));
    }

    [Fact]
    public void DateRange_ValidatesIntervalAndOverlap()
    {
        var now = DateTimeOffset.UtcNow;
        var start = now;
        var end = now.AddDays(7);

        var range = DateRange.Create(start, end);

        Assert.False(range.IsOpenEnded);
        Assert.Equal(TimeSpan.FromDays(7), range.Duration);
        Assert.True(range.Contains(now.AddDays(3)));
        Assert.False(range.Contains(now.AddDays(10)));

        var overlapping = DateRange.Create(now.AddDays(5), now.AddDays(12));
        Assert.True(range.Overlaps(overlapping));

        var nonOverlapping = DateRange.Create(now.AddDays(10), now.AddDays(15));
        Assert.False(range.Overlaps(nonOverlapping));
    }

    [Fact]
    public void DateRange_ThrowsWhenEndPrecedesStart()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.Throws<ArgumentException>(() => DateRange.Create(now, now.AddDays(-1)));
    }

    [Fact]
    public void Percentage_ComputesFactorAndAppliesCorrectly()
    {
        var p = Percentage.Create(15.5m);

        Assert.Equal(15.5m, p.Value);
        Assert.Equal(0.155m, p.Factor);
        Assert.Equal(15.50m, p.ApplyTo(100m));

        var fromFraction = Percentage.FromFraction(0.25m);
        Assert.Equal(25m, fromFraction.Value);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(100.1)]
    public void Percentage_ThrowsWhenOutOfBounds(decimal value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Percentage.Create(value));
    }

    [Fact]
    public void TenantIdentifier_ValidatesSlugConstraints()
    {
        var id = TenantIdentifier.Create("acme-corp-1");

        Assert.Equal("acme-corp-1", id.Value);
        Assert.Equal("acme-corp-1", (string)id);

        Assert.True(TenantIdentifier.TryCreate("valid-slug", out var parsed));
        Assert.NotNull(parsed);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("-invalid")]
    [InlineData("invalid-")]
    [InlineData("has spaces")]
    [InlineData("has_underscores")]
    public void TenantIdentifier_ThrowsOnInvalidInput(string input)
    {
        Assert.Throws<ArgumentException>(() => TenantIdentifier.Create(input));
        Assert.False(TenantIdentifier.TryCreate(input, out _));
    }

    [Fact]
    public void TenantEmailSettings_ValidatesSettingsCorrectly()
    {
        var settings = TenantEmailSettings.Create(
            smtpHost: "smtp.office365.com",
            smtpPort: 587,
            fromEmail: "notifications@company.com",
            fromName: "Notifications",
            username: "user@company.com",
            passwordEncrypted: "encrypted-token",
            enableSsl: true);

        Assert.Equal("smtp.office365.com", settings.SmtpHost);
        Assert.Equal(587, settings.SmtpPort);
        Assert.Equal("notifications@company.com", settings.FromEmail);
        Assert.Equal("Notifications", settings.FromName);
        Assert.True(settings.EnableSsl);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(70000)]
    public void TenantEmailSettings_ThrowsOnInvalidPort(int port)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TenantEmailSettings.Create(
            smtpHost: "smtp.example.com",
            smtpPort: port,
            fromEmail: "admin@example.com",
            fromName: "Admin"));
    }

    [Fact]
    public void FullName_NormalizesAndProducesDisplayName()
    {
        var fullName = FullName.Create("  John ", " Doe  ");

        Assert.Equal("John", fullName.FirstName);
        Assert.Equal("Doe", fullName.LastName);
        Assert.Equal("John Doe", fullName.DisplayName);
    }

    [Fact]
    public void StronglyTypedIds_BehaveAsGuidsAndSupportEquality()
    {
        var rawGuid = Guid.NewGuid();
        var tenantId1 = TenantId.From(rawGuid);
        var tenantId2 = new TenantId(rawGuid);

        Assert.Equal(tenantId1, tenantId2);
        Assert.Equal(rawGuid, (Guid)tenantId1);

        var userId = UserId.New();
        Assert.NotEqual(Guid.Empty, userId.Value);

        var roleId = RoleId.New();
        Assert.NotEqual(Guid.Empty, roleId.Value);
    }

    [Fact]
    public void Entities_EnforceValueObjectInvariants()
    {
        var tenant = new Tenant("Acme Inc", "acme-tenant");
        Assert.Equal("acme-tenant", tenant.Identifier);
        Assert.Equal("acme-tenant", tenant.ToTenantIdentifier().Value);

        var user = new User(tenant.Id, "test@example.com", "hash", "Jane", "Doe");
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("test@example.com", user.ToEmail().Value);
        Assert.Equal("Jane Doe", user.ToFullName().DisplayName);

        user.UpdateProfile("Janet", "Smith");
        Assert.Equal("Janet Smith", user.ToFullName().DisplayName);
    }
}
