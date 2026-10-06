using FluentAssertions;
using ModularSaaS.Security.Cryptography;
using Xunit;

namespace ModularSaaS.Unit.Tests.Security;

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new(workFactor: 10);

    [Fact]
    public void Hash_GeneratesValidBCryptHash()
    {
        var password = "SecurePassword123!";

        var hash = _hasher.Hash(password);

        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().StartWith("$2");
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var password = "SecurePassword123!";
        var hash = _hasher.Hash(password);

        var isValid = BCryptPasswordHasher.Verify(password, hash);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var password = "SecurePassword123!";
        var hash = _hasher.Hash(password);

        var isValid = BCryptPasswordHasher.Verify("WrongPassword!", hash);

        isValid.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithInvalidWorkFactor_ThrowsArgumentOutOfRangeException()
    {
        var act = () => new BCryptPasswordHasher(workFactor: 5);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
