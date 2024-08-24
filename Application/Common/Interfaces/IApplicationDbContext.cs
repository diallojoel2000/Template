

using Domain.Entities;

namespace Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<SampleEntity> SampleEntities { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
