namespace ModularSaaS.Application.Shared.Abstractions;

public interface IClock
{
    public DateTimeOffset UtcNow { get; }
}
