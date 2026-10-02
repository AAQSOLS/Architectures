using FluentValidation;
using ModularSaaS.Application.Shared.Constants;
using ModularSaaS.Application.Tenancy.Models;

namespace ModularSaaS.Application.Tenancy.Validators;

internal sealed class CreateTenantValidator : AbstractValidator<CreateTenantInput>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationConstants.Messages.TenantNameRequired)
            .MaximumLength(ValidationConstants.MaxTenantNameLength).WithMessage(ValidationConstants.Messages.TenantNameMaxLength);

        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage(ValidationConstants.Messages.TenantIdentifierRequired)
            .MaximumLength(ValidationConstants.MaxTenantIdentifierLength).WithMessage(ValidationConstants.Messages.TenantIdentifierMaxLength)
            .Matches(ValidationConstants.TenantIdentifierRegex).WithMessage(ValidationConstants.Messages.TenantIdentifierInvalid);

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage(ValidationConstants.Messages.AdminEmailRequired)
            .EmailAddress().WithMessage(ValidationConstants.Messages.EmailInvalid);

        RuleFor(x => x.AdminPassword)
            .NotEmpty().WithMessage(ValidationConstants.Messages.AdminPasswordRequired)
            .MinimumLength(ValidationConstants.MinPasswordLength).WithMessage(ValidationConstants.Messages.AdminPasswordMinLength);

        RuleFor(x => x.AdminFirstName)
            .NotEmpty().WithMessage(ValidationConstants.Messages.AdminFirstNameRequired)
            .MaximumLength(ValidationConstants.MaxNameLength);

        RuleFor(x => x.AdminLastName)
            .NotEmpty().WithMessage(ValidationConstants.Messages.AdminLastNameRequired)
            .MaximumLength(ValidationConstants.MaxNameLength);
    }
}
