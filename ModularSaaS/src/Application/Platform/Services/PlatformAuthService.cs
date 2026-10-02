using FluentValidation;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Platform.Abstractions;
using ModularSaaS.Application.Platform.Errors;
using ModularSaaS.Application.Platform.Models;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Application.Shared.Constants;
using ModularSaaS.Application.Shared.Models;
using ModularSaaS.Domain.Identity.Enums;

namespace ModularSaaS.Application.Platform.Services;

internal sealed class PlatformAuthService(
    IPlatformUserRepository platformUserRepository,
    IUserImpersonationService userImpersonationService,
    IPlatformTokenService platformTokenService,
    IPasswordHasher passwordHasher,
    ICurrentUser currentUser,
    IClock clock,
    IValidator<PlatformLoginInput> loginValidator) : IPlatformAuthService
{
    private const string DefaultActorEmail = "platform-admin";

    public async Task<Result<PlatformAuthResult>> LoginAsync(PlatformLoginInput input, CancellationToken ct = default)
    {
        var validation = await loginValidator.ValidateAsync(input, ct);
        if (!validation.IsValid)
        {
            return Result.Failure<PlatformAuthResult>(PlatformAuthErrors.Validation(validation.Errors[0].ErrorMessage));
        }

        var platformUser = await platformUserRepository.GetByEmailAsync(input.Email, ct);
        if (platformUser is null || platformUser.Status != UserStatus.Active)
        {
            return Result.Failure<PlatformAuthResult>(PlatformAuthErrors.InvalidCredentials);
        }

        if (!passwordHasher.Verify(input.Password, platformUser.PasswordHash))
        {
            return Result.Failure<PlatformAuthResult>(PlatformAuthErrors.InvalidCredentials);
        }

        var fullName = $"{platformUser.FirstName} {platformUser.LastName}".Trim();
        var token = platformTokenService.GeneratePlatformToken(platformUser.Id, platformUser.Email, fullName);

        return new PlatformAuthResult(token, platformUser.Id, platformUser.Email, fullName);
    }

    public async Task<Result<ImpersonationResult>> ImpersonateAsync(Guid targetTenantId, ImpersonateInput input, CancellationToken ct = default)
    {
        if (!currentUser.IsPlatformAdmin || !currentUser.UserId.HasValue)
        {
            return Result.Failure<ImpersonationResult>(PlatformAuthErrors.Forbidden);
        }

        var impersonationDataResult = await userImpersonationService.GetImpersonationDataAsync(targetTenantId, input.TargetUserId, ct);
        if (impersonationDataResult.IsFailure)
        {
            return Result.Failure<ImpersonationResult>(PlatformAuthErrors.TargetUserNotFound);
        }

        var impersonationData = impersonationDataResult.Value;
        var actorId = currentUser.UserId.Value;
        var actorEmail = currentUser.Email ?? DefaultActorEmail;

        var token = platformTokenService.GenerateImpersonationToken(
            impersonationData.UserId,
            targetTenantId,
            actorId,
            actorEmail,
            impersonationData.Roles,
            impersonationData.Permissions);

        var expiresAtUtc = clock.UtcNow.AddMinutes(SecurityPolicies.LockoutDurationMinutes);
        return new ImpersonationResult(token, expiresAtUtc);
    }
}
