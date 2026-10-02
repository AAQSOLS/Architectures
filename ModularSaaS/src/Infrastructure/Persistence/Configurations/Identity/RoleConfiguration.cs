using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularSaaS.Domain.Identity;

namespace ModularSaaS.Infrastructure.Persistence.Configurations.Identity;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles", "identity");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.TenantId)
            .IsRequired();

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(r => new { r.TenantId, r.Name })
            .IsUnique();

        builder.Property(r => r.Description)
            .HasMaxLength(250);

        builder.Property(r => r.IsSystem)
            .IsRequired();

        builder.Property(r => r.IsDefault)
            .IsRequired();
    }
}
