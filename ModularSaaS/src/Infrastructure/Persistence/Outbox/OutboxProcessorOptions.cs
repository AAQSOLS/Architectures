using System.ComponentModel.DataAnnotations;

namespace ModularSaaS.Infrastructure.Persistence.Outbox;

internal sealed class OutboxProcessorOptions
{
    public const string SectionName = "Outbox";

    [Range(1, 3600)]
    public int PollingIntervalSeconds { get; set; } = 10;

    [Range(1, 500)]
    public int BatchSize { get; set; } = 20;

    [Range(1, 20)]
    public int MaxRetryCount { get; set; } = 5;
}
