using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Domain.Identity.Events;

namespace ModularSaaS.Application.Identity.EventHandlers;

internal sealed class UserCreatedWelcomeNotificationHandler(IEmailSender emailSender) : IDomainEventHandler<UserCreatedDomainEvent>
{
    public async Task HandleAsync(UserCreatedDomainEvent domainEvent, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var subject = "Welcome to ModularSaaS!";
        var body = $"Hello {domainEvent.FirstName},\n\nYour account has been created successfully. Welcome to ModularSaaS!";

        await emailSender.SendAsync(domainEvent.Email, subject, body, ct);
    }
}
