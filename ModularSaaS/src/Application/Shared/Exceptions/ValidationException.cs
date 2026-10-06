using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Application.Shared.Exceptions;

public sealed class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = [];
    }

    public ValidationException(string? message)
        : base(message)
    {
        Errors = [];
    }

    public ValidationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
        Errors = [];
    }

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
