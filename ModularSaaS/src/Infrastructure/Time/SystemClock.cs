using ModularSaaS.Application.Shared.Abstractions;

namespace ModularSaaS.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
#pragma warning disable RS0030 // SystemClock is the sole authorized provider of system time
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
#pragma warning restore RS0030
}
