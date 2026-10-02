namespace ModularSaaS.Application.Shared.Exceptions;

public sealed class UnauthorizedException(string message = "You are not authorized to perform this action.")
    : Exception(message)
{
}
