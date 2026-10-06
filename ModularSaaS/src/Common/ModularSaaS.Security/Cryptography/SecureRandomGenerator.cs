using System.Security.Cryptography;
using System.Text;

namespace ModularSaaS.Security.Cryptography;

public sealed class SecureRandomGenerator : ISecureRandomGenerator
{
    public string CreateUrlSafeToken(int byteLength = 32)
    {
        if (byteLength < 16)
        {
            throw new ArgumentOutOfRangeException(nameof(byteLength), "Byte length must be at least 16 bytes for security.");
        }

        var bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public string CreateHexString(int byteLength = 32)
    {
        if (byteLength < 16)
        {
            throw new ArgumentOutOfRangeException(nameof(byteLength), "Byte length must be at least 16 bytes for security.");
        }

        var bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public string CreateNumericCode(int digits = 6)
    {
        if (digits < 4 || digits > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(digits), "Digits must be between 4 and 12.");
        }

        var min = (int)Math.Pow(10, digits - 1);
        var max = (int)Math.Pow(10, digits) - 1;
        var value = RandomNumberGenerator.GetInt32(min, max + 1);
        return value.ToString($"D{digits}", System.Globalization.CultureInfo.InvariantCulture);
    }

    public bool ConstantTimeEquals(string a, string b)
    {
        if (a is null || b is null)
        {
            return ReferenceEquals(a, b);
        }

        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);

        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }
}
