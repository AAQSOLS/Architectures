using ModularSaaS.Domain.Shared;
using ModularSaaS.Domain.Shared.ValueObjects;

namespace ModularSaaS.Domain.Tenancy.ValueObjects;

public sealed record TenantEmailSettings : ValueObject
{
    public string SmtpHost { get; init; }

    public int SmtpPort { get; init; }

    public string? Username { get; init; }

    public string? PasswordEncrypted { get; init; }

    public string FromEmail { get; init; }

    public string FromName { get; init; }

    public bool EnableSsl { get; init; }

    private TenantEmailSettings(
        string smtpHost,
        int smtpPort,
        string? username,
        string? passwordEncrypted,
        string fromEmail,
        string fromName,
        bool enableSsl)
    {
        SmtpHost = smtpHost;
        SmtpPort = smtpPort;
        Username = username;
        PasswordEncrypted = passwordEncrypted;
        FromEmail = fromEmail;
        FromName = fromName;
        EnableSsl = enableSsl;
    }

    public static TenantEmailSettings Create(
        string smtpHost,
        int smtpPort,
        string fromEmail,
        string fromName,
        string? username = null,
        string? passwordEncrypted = null,
        bool enableSsl = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(smtpHost);
        ArgumentException.ThrowIfNullOrWhiteSpace(fromEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(fromName);

        if (smtpPort is < 1 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(smtpPort), smtpPort, "SMTP port must be between 1 and 65535.");
        }

        var validatedEmail = Email.Create(fromEmail);

        return new TenantEmailSettings(
            smtpHost.Trim(),
            smtpPort,
            string.IsNullOrWhiteSpace(username) ? null : username.Trim(),
            passwordEncrypted,
            validatedEmail.Value,
            fromName.Trim(),
            enableSsl);
    }
}
