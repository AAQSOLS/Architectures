using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Application.Identity.Abstractions;

public interface IUserRegistrationService
{
    public Task<Result<UserResult>> RegisterUserAsync(RegisterUserInput input, CancellationToken ct = default);
}
