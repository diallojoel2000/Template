using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Infrastructure.Persistence.Configurations;
internal class AppRolePermissionConfiguration : IEntityTypeConfiguration<AppRolePermission>
{
    public void Configure(EntityTypeBuilder<AppRolePermission> builder)
    {
        builder.HasKey(m => new { m.AppRoleId, m.PermissionId });
        builder.Property(m => m.PermissionId).HasMaxLength(37);

        builder.HasOne(m=>m.AppRole).WithMany(m=>m.RolePermissions).HasForeignKey(m =>m.AppRoleId);
    }
}
