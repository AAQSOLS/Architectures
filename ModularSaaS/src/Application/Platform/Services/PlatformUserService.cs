using FluentValidation;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Platform.Abstractions;
using ModularSaaS.Application.Platform.Errors;
using ModularSaaS.Application.Platform.Models;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Application.Shared.Models;
using ModularSaaS.Domain.Identity.Enums;
using ModularSaaS.Domain.Platform;

namespace ModularSaaS.Application.Platform.Services;

internal sealed class PlatformUserService(
    IPlatformUserRepository platformUserRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IValidator<CreatePlatformAdminInput> createValidator) : IPlatformUserService
{
    public async Task<Result<IReadOnlyList<PlatformAdminResult>>> ListAdminsAsync(CancellationToken ct = default)
    {
        var admins = await platformUserRepository.ListAsync(ct);
        var results = admins.Select(a => new PlatformAdminResult(
            a.Id,
            a.Email,
            a.FirstName,
            a.LastName,
            a.Status,
            a.CreatedAtUtc)).ToList();

        return results;
    }

    public async Task<Result<PlatformAdminResult>> CreateAdminAsync(CreatePlatformAdminInput input, CancellationToken ct = default)
    {
        var validation = await createValidator.ValidateAsync(input, ct);
        if (!validation.IsValid)
        {
            return Result.Failure<PlatformAdminResult>(PlatformAdminErrors.Validation(validation.Errors[0].ErrorMessage));
        }

        var normalizedEmail = input.Email.Trim().ToLowerInvariant();
        if (await platformUserRepository.ExistsAsync(normalizedEmail, ct))
        {
            return Result.Failure<PlatformAdminResult>(PlatformAdminErrors.EmailExists);
        }

        var passwordHash = passwordHasher.Hash(input.Password);
        var admin = new PlatformUser(normalizedEmail, passwordHash, input.FirstName.Trim(), input.LastName.Trim());

        await platformUserRepository.AddAsync(admin, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new PlatformAdminResult(
            admin.Id,
            admin.Email,
            admin.FirstName,
            admin.LastName,
            admin.Status,
            admin.CreatedAtUtc);
    }

    public async Task<Result> DeactivateAdminAsync(Guid id, CancellationToken ct = default)
    {
        if (currentUser.UserId.HasValue && currentUser.UserId.Value == id)
        {
            return Result.Failure(PlatformAdminErrors.CannotDeactivateSelf);
        }

        var admin = await platformUserRepository.GetByIdAsync(id, ct);
        if (admin is null)
        {
            return Result.Failure(PlatformAdminErrors.NotFound);
        }

        if (admin.Status == UserStatus.Inactive)
        {
            return Result.Failure(PlatformAdminErrors.AlreadyInactive);
        }

        var activeCount = await platformUserRepository.CountActiveAsync(ct);
        if (activeCount <= 1)
        {
            return Result.Failure(PlatformAdminErrors.CannotDeactivateLastSuperAdmin);
        }

        admin.Deactivate();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> ActivateAdminAsync(Guid id, CancellationToken ct = default)
    {
        var admin = await platformUserRepository.GetByIdAsync(id, ct);
        if (admin is null)
        {
            return Result.Failure(PlatformAdminErrors.NotFound);
        }

        if (admin.Status == UserStatus.Active)
        {
            return Result.Failure(PlatformAdminErrors.AlreadyActive);
        }

        admin.Activate();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
