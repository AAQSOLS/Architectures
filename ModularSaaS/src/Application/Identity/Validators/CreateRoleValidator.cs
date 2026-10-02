using FluentValidation;
using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Constants;

namespace ModularSaaS.Application.Identity.Validators;

internal sealed class CreateRoleValidator : AbstractValidator<CreateRoleInput>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationConstants.Messages.RoleNameRequired)
            .MaximumLength(ValidationConstants.MaxNameLength);

        RuleFor(x => x.Description)
            .MaximumLength(ValidationConstants.MaxDescriptionLength);

        RuleFor(x => x.Permissions)
            .NotNull().WithMessage(ValidationConstants.Messages.PermissionsRequired);
    }
}
