namespace ModularSaaS.Application.Shared.Models;

public sealed record ValidationError(string PropertyName, string ErrorMessage);
