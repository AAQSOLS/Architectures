namespace ModularSaaS.Security.Abstractions;

public interface ISecurityEventSink
{
    public ValueTask EmitAsync<TEvent>(TEvent securityEvent, CancellationToken ct = default)
        where TEvent : SecurityEvent;
}
