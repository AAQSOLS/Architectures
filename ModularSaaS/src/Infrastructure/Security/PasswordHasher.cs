using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Security.Cryptography;

namespace ModularSaaS.Infrastructure.Security;

internal sealed class PasswordHasher(BCryptPasswordHasher hasher) : IPasswordHasher
{
    public string Hash(string password) => hasher.Hash(password);

    public bool Verify(string password, string passwordHash) => BCryptPasswordHasher.Verify(password, passwordHash);
}
