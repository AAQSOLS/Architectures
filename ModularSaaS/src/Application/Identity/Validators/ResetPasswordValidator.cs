using FluentValidation;
using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Constants;

namespace ModularSaaS.Application.Identity.Validators;

internal sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordInput>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationConstants.Messages.EmailRequired)
            .EmailAddress().WithMessage(ValidationConstants.Messages.EmailInvalid);

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(ValidationConstants.Messages.ResetTokenRequired);

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(ValidationConstants.Messages.NewPasswordRequired)
            .MinimumLength(ValidationConstants.MinPasswordLength).WithMessage(ValidationConstants.Messages.NewPasswordMinLength);
    }
}
