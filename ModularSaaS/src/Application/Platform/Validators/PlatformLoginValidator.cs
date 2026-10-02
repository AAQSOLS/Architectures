using FluentValidation;
using ModularSaaS.Application.Platform.Models;
using ModularSaaS.Application.Shared.Constants;

namespace ModularSaaS.Application.Platform.Validators;

internal sealed class PlatformLoginValidator : AbstractValidator<PlatformLoginInput>
{
    public PlatformLoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationConstants.Messages.EmailRequired)
            .EmailAddress().WithMessage(ValidationConstants.Messages.EmailInvalid);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationConstants.Messages.PasswordRequired);
    }
}
