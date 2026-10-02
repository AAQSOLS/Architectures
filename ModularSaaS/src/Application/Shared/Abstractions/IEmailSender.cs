namespace ModularSaaS.Application.Shared.Abstractions;

public interface IEmailSender
{
    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
}
