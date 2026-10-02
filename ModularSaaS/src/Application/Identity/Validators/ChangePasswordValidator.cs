using FluentValidation;
using ModularSaaS.Application.Identity.Models;

namespace ModularSaaS.Application.Identity.Validators;

internal sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordInput>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters long.");
    }
}
