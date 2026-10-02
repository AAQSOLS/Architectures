using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularSaaS.Domain.Platform;

namespace ModularSaaS.Infrastructure.Persistence.Configurations.Platform;

internal sealed class PlatformUserConfiguration : IEntityTypeConfiguration<PlatformUser>
{
    public void Configure(EntityTypeBuilder<PlatformUser> builder)
    {
        builder.ToTable("PlatformUsers", "identity");

        builder.HasKey(pu => pu.Id);

        builder.Property(pu => pu.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(pu => pu.Email)
            .IsUnique();

        builder.Property(pu => pu.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(pu => pu.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pu => pu.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pu => pu.Status)
            .IsRequired();

        builder.Property(pu => pu.CreatedAtUtc)
            .IsRequired();
    }
}
