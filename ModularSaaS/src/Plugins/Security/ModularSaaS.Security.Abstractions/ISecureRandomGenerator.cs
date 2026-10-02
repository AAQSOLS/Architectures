namespace ModularSaaS.Security.Abstractions;

public interface ISecureRandomGenerator
{
    public string CreateUrlSafeToken(int byteLength = 32);

    public string CreateHexString(int byteLength = 32);

    public string CreateNumericCode(int digits = 6);

    public bool ConstantTimeEquals(string a, string b);
}
