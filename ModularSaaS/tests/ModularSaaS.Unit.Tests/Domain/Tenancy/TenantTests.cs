using FluentAssertions;
using ModularSaaS.Domain.Tenancy;
using ModularSaaS.Domain.Tenancy.Enums;
using Xunit;

namespace ModularSaaS.Unit.Tests.Domain.Tenancy;

public class TenantTests
{
    [Fact]
    public void Constructor_WithValidArguments_InitializesTenantInActiveState()
    {
        var tenant = new Tenant("Acme Corp", "acme", TenantPlan.Professional);

        tenant.Name.Should().Be("Acme Corp");
        tenant.Identifier.Should().Be("acme");
        tenant.Plan.Should().Be(TenantPlan.Professional);
        tenant.Status.Should().Be(TenantStatus.Active);
    }

    [Theory]
    [InlineData("", "identifier")]
    [InlineData("   ", "identifier")]
    [InlineData(null, "identifier")]
    [InlineData("Name", "")]
    [InlineData("Name", "   ")]
    [InlineData("Name", null)]
    public void Constructor_WithInvalidArguments_ThrowsArgumentException(string? name, string? identifier)
    {
        var act = () => new Tenant(name!, identifier!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Suspend_WhenActive_ChangesStatusToSuspended()
    {
        var tenant = new Tenant("Acme Corp", "acme");

        tenant.Suspend();

        tenant.Status.Should().Be(TenantStatus.Suspended);
    }

    [Fact]
    public void Activate_WhenSuspended_ChangesStatusToActive()
    {
        var tenant = new Tenant("Acme Corp", "acme");
        tenant.Suspend();

        tenant.Activate();

        tenant.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public void ChangePlan_UpdatesPlan()
    {
        var tenant = new Tenant("Acme Corp", "acme", TenantPlan.Free);

        tenant.ChangePlan(TenantPlan.Enterprise);

        tenant.Plan.Should().Be(TenantPlan.Enterprise);
    }
}
