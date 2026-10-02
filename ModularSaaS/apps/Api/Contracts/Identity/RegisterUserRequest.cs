namespace ModularSaaS.Api.Contracts.Identity;

public sealed record RegisterUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    Guid? RoleId = null);

