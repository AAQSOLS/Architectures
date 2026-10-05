namespace ModularSaaS.Infrastructure.Persistence.Outbox;

internal sealed class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTimeOffset OccurredOnUtc { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset? ProcessedOnUtc { get; set; }

    public string? Error { get; set; }

    public int RetryCount { get; set; }
}
