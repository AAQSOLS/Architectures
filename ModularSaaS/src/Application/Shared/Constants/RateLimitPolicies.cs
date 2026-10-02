namespace ModularSaaS.Application.Shared.Constants;

public static class RateLimitPolicies
{
    public const string AuthStrict = "AuthStrict";
    public const string General = "General";

    public const int AuthStrictPermitLimit = 5;
    public const int AuthStrictWindowMinutes = 1;
    public const int AuthStrictQueueLimit = 0;

    public const int GeneralPermitLimit = 100;
    public const int GeneralWindowMinutes = 1;
    public const int GeneralQueueLimit = 2;
}
