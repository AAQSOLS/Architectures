using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularSaaS.Domain.Identity;

namespace ModularSaaS.Infrastructure.Persistence.Configurations.Identity;

internal sealed class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("UserPermissions", "identity");

        builder.HasKey(up => new { up.UserId, up.PermissionId });

        builder.Property(up => up.IsGranted)
            .IsRequired();

        builder.HasOne(up => up.User)
            .WithMany(u => u.Permissions)
            .HasForeignKey(up => up.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(up => up.Permission)
            .WithMany()
            .HasForeignKey(up => up.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
