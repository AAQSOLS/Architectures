using Microsoft.Extensions.Logging;
using ModularSaaS.Application.Shared.Abstractions;

namespace ModularSaaS.Infrastructure.Communications;

internal sealed class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        logger.LogInformation("Sending email to {Recipient}: Subject: {Subject} | Body: {Body}", to, subject, body);
        return Task.CompletedTask;
    }
}
