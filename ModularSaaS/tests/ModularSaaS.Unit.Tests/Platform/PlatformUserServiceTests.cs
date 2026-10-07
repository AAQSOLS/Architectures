using FluentAssertions;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Platform.Abstractions;
using ModularSaaS.Application.Platform.Errors;
using ModularSaaS.Application.Platform.Models;
using ModularSaaS.Application.Platform.Services;
using ModularSaaS.Application.Platform.Validators;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Domain.Identity.Enums;
using ModularSaaS.Domain.Platform;
using NSubstitute;
using Xunit;

namespace ModularSaaS.Unit.Tests.Platform;

public class PlatformUserServiceTests
{
    private readonly IPlatformUserRepository _repository = Substitute.For<IPlatformUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly CreatePlatformAdminValidator _validator = new();

    private PlatformUserService CreateSut() =>
        new(_repository, _passwordHasher, _unitOfWork, _currentUser, _validator);

    [Fact]
    public async Task DeactivateAdminAsync_WhenTargetIsCaller_ReturnsCannotDeactivateSelf()
    {
        var callerId = Guid.NewGuid();
        _currentUser.UserId.Returns(callerId);
        var sut = CreateSut();

        var result = await sut.DeactivateAdminAsync(callerId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PlatformAdminErrors.CannotDeactivateSelf);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateAdminAsync_WhenTargetIsLastActiveSuperAdmin_ReturnsCannotDeactivateLastSuperAdmin()
    {
        var callerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        _currentUser.UserId.Returns(callerId);

        var targetAdmin = new PlatformUser("admin2@platform.local", "hash", "Target", "Admin");
        _repository.GetByIdAsync(targetId).Returns(targetAdmin);
        _repository.CountActiveAsync().Returns(1);

        var sut = CreateSut();

        var result = await sut.DeactivateAdminAsync(targetId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PlatformAdminErrors.CannotDeactivateLastSuperAdmin);
        targetAdmin.Status.Should().Be(UserStatus.Active);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateAdminAsync_WhenMultipleActiveAdminsExist_DeactivatesSuccessfully()
    {
        var callerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        _currentUser.UserId.Returns(callerId);

        var targetAdmin = new PlatformUser("admin2@platform.local", "hash", "Target", "Admin");
        _repository.GetByIdAsync(targetId).Returns(targetAdmin);
        _repository.CountActiveAsync().Returns(2);

        var sut = CreateSut();

        var result = await sut.DeactivateAdminAsync(targetId);

        result.IsSuccess.Should().BeTrue();
        targetAdmin.Status.Should().Be(UserStatus.Inactive);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateAdminAsync_WhenTargetNotFound_ReturnsNotFound()
    {
        var callerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        _currentUser.UserId.Returns(callerId);
        _repository.GetByIdAsync(targetId).Returns((PlatformUser?)null);

        var sut = CreateSut();

        var result = await sut.DeactivateAdminAsync(targetId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PlatformAdminErrors.NotFound);
    }

    [Fact]
    public async Task CreateAdminAsync_WhenEmailAlreadyExists_ReturnsEmailExistsError()
    {
        _repository.ExistsAsync("newadmin@platform.local").Returns(true);
        var sut = CreateSut();

        var input = new CreatePlatformAdminInput("newadmin@platform.local", "Password123!", "John", "Doe");
        var result = await sut.CreateAdminAsync(input);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PlatformAdminErrors.EmailExists);
        await _repository.DidNotReceive().AddAsync(Arg.Any<PlatformUser>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAdminAsync_WhenValid_HashesPasswordAndPersistsAdmin()
    {
        _repository.ExistsAsync("newadmin@platform.local").Returns(false);
        _passwordHasher.Hash("Password123!").Returns("hashed_pwd");
        var sut = CreateSut();

        var input = new CreatePlatformAdminInput("newadmin@platform.local", "Password123!", "John", "Doe");
        var result = await sut.CreateAdminAsync(input);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("newadmin@platform.local");
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
        result.Value.Status.Should().Be(UserStatus.Active);

        await _repository.Received(1).AddAsync(Arg.Is<PlatformUser>(u => u.Email == "newadmin@platform.local"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActivateAdminAsync_WhenInactive_ActivatesSuccessfully()
    {
        var targetId = Guid.NewGuid();
        var targetAdmin = new PlatformUser("admin@platform.local", "hash", "Target", "Admin");
        targetAdmin.Deactivate();
        _repository.GetByIdAsync(targetId).Returns(targetAdmin);

        var sut = CreateSut();

        var result = await sut.ActivateAdminAsync(targetId);

        result.IsSuccess.Should().BeTrue();
        targetAdmin.Status.Should().Be(UserStatus.Active);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
