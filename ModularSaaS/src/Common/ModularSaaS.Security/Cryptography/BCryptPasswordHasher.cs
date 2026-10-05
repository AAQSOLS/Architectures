namespace ModularSaaS.Security.Cryptography;

public sealed class BCryptPasswordHasher
{
    private readonly int _workFactor;

    public BCryptPasswordHasher(int workFactor = 12)
    {
        if (workFactor < 10 || workFactor > 31)
        {
            throw new ArgumentOutOfRangeException(nameof(workFactor), "BCrypt work factor must be between 10 and 31.");
        }

        _workFactor = workFactor;
    }

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, _workFactor);
    }

    public static bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public PasswordVerificationResult VerifyWithRehashCheck(string password, string passwordHash)
    {
        if (!Verify(password, passwordHash))
        {
            return PasswordVerificationResult.Failed;
        }

        return NeedsRehash(passwordHash)
            ? PasswordVerificationResult.SuccessRehashNeeded
            : PasswordVerificationResult.Success;
    }

    public bool NeedsRehash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return true;
        }

        var parts = passwordHash.Split('$');
        if (parts.Length > 2 && int.TryParse(parts[2], out var currentWorkFactor))
        {
            return currentWorkFactor < _workFactor;
        }

        return false;
    }
}
