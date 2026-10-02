using FluentValidation;
using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Constants;

namespace ModularSaaS.Application.Identity.Validators;

internal sealed class LoginValidator : AbstractValidator<LoginInput>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationConstants.Messages.EmailRequired)
            .EmailAddress().WithMessage(ValidationConstants.Messages.EmailInvalid);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationConstants.Messages.PasswordRequired);
    }
}
