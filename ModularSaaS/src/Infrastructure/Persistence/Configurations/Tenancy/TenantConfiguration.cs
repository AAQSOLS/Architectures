using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularSaaS.Domain.Tenancy;

namespace ModularSaaS.Infrastructure.Persistence.Configurations.Tenancy;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants", "tenancy");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(t => t.Identifier)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(t => t.Identifier)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.Property(t => t.Status)
            .IsRequired();

        builder.Property(t => t.Plan)
            .IsRequired();
    }
}
