using FluentValidation;
using Mapster;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Application.Shared.Models;
using ModularSaaS.Application.Tenancy.Abstractions;
using ModularSaaS.Application.Tenancy.Errors;
using ModularSaaS.Application.Tenancy.Models;
using ModularSaaS.Domain.Tenancy;

namespace ModularSaaS.Application.Tenancy.Services;

internal sealed class TenantService(
    ITenantRepository tenantRepository,
    IUserRegistrationService userRegistrationService,
    ITenantLookupService tenantLookupService,
    IUnitOfWork unitOfWork,
    IValidator<CreateTenantInput> createTenantValidator) : ITenantService
{
    public async Task<Result<TenantResult>> CreateTenantAsync(CreateTenantInput input, CancellationToken ct = default)
    {
        var validation = await createTenantValidator.ValidateAsync(input, ct);
        if (!validation.IsValid)
        {
            return Result.Failure<TenantResult>(TenancyErrors.Validation(validation.Errors[0].ErrorMessage));
        }

        var slug = input.Identifier.Trim().ToLowerInvariant();
        if (await tenantRepository.IdentifierExistsAsync(slug, ct))
        {
            return Result.Failure<TenantResult>(TenancyErrors.IdentifierExists);
        }

        var tenant = new Tenant(input.Name, slug, input.Plan);
        await tenantRepository.AddAsync(tenant, ct);
        await unitOfWork.SaveChangesAsync(ct);

        // Register initial tenant administrator using IUserRegistrationService abstraction (defaults to tenant admin role)
        var registerResult = await userRegistrationService.RegisterUserAsync(new RegisterUserInput(
            input.AdminEmail,
            input.AdminPassword,
            input.AdminFirstName,
            input.AdminLastName,
            RoleId: null,
            TenantId: tenant.Id), ct);

        if (registerResult.IsFailure)
        {
            return Result.Failure<TenantResult>(registerResult.Error);
        }

        return tenant.Adapt<TenantResult>();
    }

    public async Task<Result<TenantResult>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await tenantRepository.GetByIdAsync(id, ct);
        if (tenant is null)
        {
            return Result.Failure<TenantResult>(TenancyErrors.NotFound);
        }

        return tenant.Adapt<TenantResult>();
    }

    public async Task<Result<TenantResult>> GetByIdentifierAsync(string identifier, CancellationToken ct = default)
    {
        var tenant = await tenantRepository.GetByIdentifierAsync(identifier, ct);
        if (tenant is null)
        {
            return Result.Failure<TenantResult>(TenancyErrors.NotFound);
        }

        return tenant.Adapt<TenantResult>();
    }

    public async Task<Result<PagedResult<TenantListItem>>> ListTenantsAsync(TenantFilter filter, CancellationToken ct = default)
    {
        var result = await tenantRepository.ListAsync(filter, ct);
        return result;
    }

    public async Task<Result> SuspendTenantAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await tenantRepository.GetByIdAsync(id, ct);
        if (tenant is null)
        {
            return Result.Failure(TenancyErrors.NotFound);
        }

        tenant.Suspend();
        await unitOfWork.SaveChangesAsync(ct);
        await tenantLookupService.InvalidateAsync(id, ct);
        return Result.Success();
    }

    public async Task<Result> ActivateTenantAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await tenantRepository.GetByIdAsync(id, ct);
        if (tenant is null)
        {
            return Result.Failure(TenancyErrors.NotFound);
        }

        tenant.Activate();
        await unitOfWork.SaveChangesAsync(ct);
        await tenantLookupService.InvalidateAsync(id, ct);
        return Result.Success();
    }
}
