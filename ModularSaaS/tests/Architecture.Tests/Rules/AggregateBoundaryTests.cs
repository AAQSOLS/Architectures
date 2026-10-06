using ModularSaaS.Domain.Identity;
using ModularSaaS.Domain.Identity.Enums;
using ModularSaaS.Domain.Identity.Events;
using ModularSaaS.Domain.Tenancy;
using ModularSaaS.Domain.Tenancy.Enums;
using ModularSaaS.Domain.Tenancy.Events;
using Xunit;

namespace ModularSaaS.Architecture.Tests.Rules;

public class AggregateBoundaryTests
{
    [Fact]
    public void User_AssignRole_AddsRoleAndRaisesDomainEvent()
    {
        var tenantId = Guid.NewGuid();
        var user = new User(tenantId, "alice@test.com", "hash", "Alice", "Smith");
        user.ClearDomainEvents();

        var roleId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        user.AssignRole(roleId, now);

        Assert.Single(user.Roles);
        Assert.Equal(roleId, user.Roles.First().RoleId);

        var domainEvent = Assert.Single(user.DomainEvents.OfType<UserRoleAssignedDomainEvent>());
        Assert.Equal(user.Id, domainEvent.UserId);
        Assert.Equal(tenantId, domainEvent.TenantId);
        Assert.Equal(roleId, domainEvent.RoleId);
    }

    [Fact]
    public void User_AssignRole_ThrowsWhenUserIsNotActive()
    {
        var user = new User(Guid.NewGuid(), "inactive@test.com", "hash", "Inactive", "User");
        user.Deactivate();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            user.AssignRole(Guid.NewGuid(), DateTimeOffset.UtcNow));

        Assert.Contains("status", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void User_RemoveRole_RemovesRoleAndRaisesDomainEvent()
    {
        var user = new User(Guid.NewGuid(), "bob@test.com", "hash", "Bob", "Jones");
        var roleId = Guid.NewGuid();
        user.AssignRole(roleId, DateTimeOffset.UtcNow);
        user.ClearDomainEvents();

        user.RemoveRole(roleId);

        Assert.Empty(user.Roles);
        var domainEvent = Assert.Single(user.DomainEvents.OfType<UserRoleRemovedDomainEvent>());
        Assert.Equal(roleId, domainEvent.RoleId);
    }

    [Fact]
    public void User_SetDirectPermission_AddsOrUpdatesPermissionAndRaisesDomainEvent()
    {
        var user = new User(Guid.NewGuid(), "charlie@test.com", "hash", "Charlie", "Brown");
        user.ClearDomainEvents();

        var permissionId = Guid.NewGuid();
        user.SetDirectPermission(permissionId, true);

        Assert.Single(user.Permissions);
        Assert.True(user.Permissions.First().IsGranted);

        var domainEvent = Assert.Single(user.DomainEvents.OfType<UserPermissionChangedDomainEvent>());
        Assert.Equal(permissionId, domainEvent.PermissionId);
        Assert.True(domainEvent.IsGranted);

        // Update to false
        user.ClearDomainEvents();
        user.SetDirectPermission(permissionId, false);

        Assert.Single(user.Permissions);
        Assert.False(user.Permissions.First().IsGranted);
        var updateEvent = Assert.Single(user.DomainEvents.OfType<UserPermissionChangedDomainEvent>());
        Assert.False(updateEvent.IsGranted);
    }

    [Fact]
    public void User_RecordFailedLogin_LocksUserWhenThresholdReached()
    {
        var user = new User(Guid.NewGuid(), "david@test.com", "hash", "David", "Miller");
        var now = DateTimeOffset.UtcNow;
        var lockoutDuration = TimeSpan.FromMinutes(15);

        user.RecordFailedLogin(3, lockoutDuration, now);
        Assert.Equal(1, user.AccessFailedCount);
        Assert.Equal(UserStatus.Active, user.Status);

        user.RecordFailedLogin(3, lockoutDuration, now);
        Assert.Equal(2, user.AccessFailedCount);

        user.RecordFailedLogin(3, lockoutDuration, now);
        Assert.Equal(3, user.AccessFailedCount);
        Assert.Equal(UserStatus.Locked, user.Status);
        Assert.NotNull(user.LockoutEndUtc);

        // Successful login resets
        user.RecordSuccessfulLogin(now.AddMinutes(20));
        Assert.Equal(0, user.AccessFailedCount);
        Assert.Null(user.LockoutEndUtc);
        Assert.Equal(UserStatus.Active, user.Status);
    }

    [Fact]
    public void Role_SetPermissions_UpdatesCollectionAndRaisesDomainEvent()
    {
        var tenantId = Guid.NewGuid();
        var role = new Role(tenantId, "Manager", "Store manager");
        role.ClearDomainEvents();

        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();

        role.SetPermissions([p1, p2]);

        Assert.Equal(2, role.Permissions.Count);
        var domainEvent = Assert.Single(role.DomainEvents.OfType<RolePermissionsUpdatedDomainEvent>());
        Assert.Equal(role.Id, domainEvent.RoleId);
        Assert.Equal(tenantId, domainEvent.TenantId);
    }

    [Fact]
    public void Role_SystemRole_ThrowsOnMutation()
    {
        var systemRole = new Role(Guid.NewGuid(), "Admin", "Admin role", isSystem: true);

        Assert.Throws<InvalidOperationException>(() =>
            systemRole.UpdateDetails("SuperAdmin", "New desc"));

        Assert.Throws<InvalidOperationException>(() =>
            systemRole.SetPermissions([Guid.NewGuid()]));

        Assert.Throws<InvalidOperationException>(() =>
            systemRole.AssignPermission(Guid.NewGuid()));
    }

    [Fact]
    public void Tenant_LifecycleMethods_UpdateStatusAndRaiseDomainEvents()
    {
        var tenant = new Tenant("Acme Corp", "acme-corp", TenantPlan.Starter);
        var createdEvent = Assert.Single(tenant.DomainEvents.OfType<TenantCreatedDomainEvent>());
        Assert.Equal("acme-corp", createdEvent.Identifier);
        Assert.Equal(TenantPlan.Starter, createdEvent.Plan);
        tenant.ClearDomainEvents();

        tenant.Suspend();
        Assert.Equal(TenantStatus.Suspended, tenant.Status);
        var suspendEvent = Assert.Single(tenant.DomainEvents.OfType<TenantStatusChangedDomainEvent>());
        Assert.Equal(TenantStatus.Suspended, suspendEvent.Status);
        tenant.ClearDomainEvents();

        tenant.Activate();
        Assert.Equal(TenantStatus.Active, tenant.Status);
        var activateEvent = Assert.Single(tenant.DomainEvents.OfType<TenantStatusChangedDomainEvent>());
        Assert.Equal(TenantStatus.Active, activateEvent.Status);
        tenant.ClearDomainEvents();

        tenant.ChangePlan(TenantPlan.Enterprise);
        Assert.Equal(TenantPlan.Enterprise, tenant.Plan);
        var planEvent = Assert.Single(tenant.DomainEvents.OfType<TenantPlanChangedDomainEvent>());
        Assert.Equal(TenantPlan.Enterprise, planEvent.Plan);
    }
}
