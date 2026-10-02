using ModularSaaS.Security.Abstractions;

namespace ModularSaaS.Security.Core.Events;

public sealed class DefaultSecurityEventSink : ISecurityEventSink
{
    public ValueTask EmitAsync<TEvent>(TEvent securityEvent, CancellationToken ct = default)
        where TEvent : SecurityEvent
    {
        return ValueTask.CompletedTask;
    }
}
