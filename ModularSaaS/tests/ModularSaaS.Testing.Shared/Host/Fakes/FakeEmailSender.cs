using System.Collections.Concurrent;
using ModularSaaS.Application.Shared.Abstractions;

namespace ModularSaaS.Testing.Shared.Host.Fakes;

public sealed class FakeEmailSender : IEmailSender
{
    public ConcurrentBag<(string To, string Subject, string Body)> SentEmails { get; } = [];

    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        SentEmails.Add((to, subject, body));
        return Task.CompletedTask;
    }
}
