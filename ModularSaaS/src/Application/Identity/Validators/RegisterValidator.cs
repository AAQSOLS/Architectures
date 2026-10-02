using FluentValidation;
using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Constants;

namespace ModularSaaS.Application.Identity.Validators;

internal sealed class RegisterValidator : AbstractValidator<RegisterUserInput>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationConstants.Messages.EmailRequired)
            .EmailAddress().WithMessage(ValidationConstants.Messages.EmailInvalid);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationConstants.Messages.PasswordRequired)
            .MinimumLength(ValidationConstants.MinPasswordLength).WithMessage(ValidationConstants.Messages.PasswordMinLength);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(ValidationConstants.Messages.FirstNameRequired)
            .MaximumLength(ValidationConstants.MaxNameLength);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(ValidationConstants.Messages.LastNameRequired)
            .MaximumLength(ValidationConstants.MaxNameLength);
    }
}
