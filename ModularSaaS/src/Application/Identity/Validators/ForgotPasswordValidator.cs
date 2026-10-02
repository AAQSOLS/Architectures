using FluentValidation;
using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Constants;

namespace ModularSaaS.Application.Identity.Validators;

internal sealed class ForgotPasswordValidator : AbstractValidator<ForgotPasswordInput>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationConstants.Messages.EmailRequired)
            .EmailAddress().WithMessage(ValidationConstants.Messages.EmailInvalid);
    }
}
