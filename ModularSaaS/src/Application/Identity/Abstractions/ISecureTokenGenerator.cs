namespace ModularSaaS.Application.Identity.Abstractions;

public interface ISecureTokenGenerator
{
    public string GenerateRefreshToken();

    public string GeneratePasswordResetToken();

    public string HashToken(string token);
}
