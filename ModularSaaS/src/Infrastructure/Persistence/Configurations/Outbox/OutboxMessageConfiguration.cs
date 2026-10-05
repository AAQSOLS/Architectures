using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularSaaS.Infrastructure.Persistence.Outbox;

namespace ModularSaaS.Infrastructure.Persistence.Configurations.Outbox;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(m => m.Content)
            .IsRequired();

        builder.Property(m => m.Error)
            .HasMaxLength(4000);

        builder.HasIndex(m => new { m.ProcessedOnUtc, m.RetryCount, m.OccurredOnUtc })
            .HasFilter("[ProcessedOnUtc] IS NULL");
    }
}
