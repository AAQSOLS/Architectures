namespace ModularSaaS.Application.Shared.Models;

public sealed class Result
{
    private const string SuccessCannotHaveError = "Successful result cannot have an error.";
    private const string FailureMustHaveError = "Failed result must have an error.";

    private Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException(SuccessCannotHaveError);
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException(FailureMustHaveError);
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
}

public sealed class Result<T>
{
    private const string SuccessCannotHaveError = "Successful result cannot have an error.";
    private const string FailureMustHaveError = "Failed result must have an error.";
    private const string CannotAccessFailureValue = "Cannot access the value of a failure result.";

    private readonly T? _value;

    private Result(bool isSuccess, T? value, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException(SuccessCannotHaveError);
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException(FailureMustHaveError);
        }

        IsSuccess = isSuccess;
        _value = value;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(CannotAccessFailureValue);

    public static Result<T> Success(T value) => new(true, value, Error.None);

    public static Result<T> Failure(Error error) => new(false, default, error);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure(error);
}
