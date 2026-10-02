using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Application.Shared.Exceptions;

public sealed class ValidationException : Exception
{
    public ValidationException(IReadOnlyList<ValidationError> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : this([new ValidationError(propertyName, errorMessage)])
    {
    }

    public IReadOnlyList<ValidationError> Errors { get; }
}
