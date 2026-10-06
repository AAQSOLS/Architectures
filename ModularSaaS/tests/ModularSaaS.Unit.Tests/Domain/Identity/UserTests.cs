using FluentAssertions;
using ModularSaaS.Domain.Identity;
using ModularSaaS.Domain.Identity.Enums;
using ModularSaaS.Domain.Identity.Events;
using Xunit;

namespace ModularSaaS.Unit.Tests.Domain.Identity;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidArguments_InitializesUserAndRaisesDomainEvent()
    {
        var tenantId = Guid.NewGuid();

        var user = new User(tenantId, "test@domain.com", "hash123", "John", "Doe");

        user.TenantId.Should().Be(tenantId);
        user.Email.Should().Be("test@domain.com");
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Status.Should().Be(UserStatus.Active);
        user.EmailConfirmed.Should().BeFalse();
        user.AccessFailedCount.Should().Be(0);

        user.DomainEvents.Should().ContainSingle(e => e is UserCreatedDomainEvent);
        var domainEvent = (UserCreatedDomainEvent)user.DomainEvents.Single();
        domainEvent.UserId.Should().Be(user.Id);
        domainEvent.TenantId.Should().Be(tenantId);
        domainEvent.Email.Should().Be("test@domain.com");
    }

    [Fact]
    public void RecordFailedLogin_WhenExceedingMaxAttempts_LocksOutUser()
    {
        var user = new User(Guid.NewGuid(), "test@domain.com", "hash", "Jane", "Doe");
        var now = DateTimeOffset.UtcNow;
        var lockoutDuration = TimeSpan.FromMinutes(15);

        for (var i = 0; i < 5; i++)
        {
            user.RecordFailedLogin(5, lockoutDuration, now);
        }

        user.AccessFailedCount.Should().Be(5);
        user.LockoutEndUtc.Should().Be(now.Add(lockoutDuration));
    }

    [Fact]
    public void ConfirmEmail_SetsEmailConfirmedToTrue()
    {
        var user = new User(Guid.NewGuid(), "test@domain.com", "hash", "Jane", "Doe");

        user.ConfirmEmail();

        user.EmailConfirmed.Should().BeTrue();
    }
}
