namespace ModularSaaS.Security.Abstractions;

public interface IPasswordHasher
{
    public string Hash(string password);

    public bool Verify(string password, string passwordHash);

    public PasswordVerificationResult VerifyWithRehashCheck(string password, string passwordHash);

    public bool NeedsRehash(string passwordHash);
}
