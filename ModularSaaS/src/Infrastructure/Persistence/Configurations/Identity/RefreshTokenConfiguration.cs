using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularSaaS.Domain.Identity;

namespace ModularSaaS.Infrastructure.Persistence.Configurations.Identity;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens", "identity");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TenantId)
            .IsRequired();

        builder.Property(t => t.UserId)
            .IsRequired();

        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(t => t.Token)
            .IsUnique();

        builder.HasIndex(t => new { t.TenantId, t.UserId });

        builder.Property(t => t.ExpiresAtUtc)
            .IsRequired();

        builder.Property(t => t.ReplacedByToken)
            .HasMaxLength(200);

        builder.Property(t => t.CreatedByIp)
            .HasMaxLength(50);
    }
}
