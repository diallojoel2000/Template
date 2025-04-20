using Domain.Entities;

namespace Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<AppRole> AppRoles { get; }
        DbSet<AppRolePermission> AppRolePermissions { get; }
        DbSet<LoginLog> LoginLogs { get; }
        DbSet<SampleEntity> SampleEntities { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
