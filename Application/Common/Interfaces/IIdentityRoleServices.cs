using Application.Common.Models;

namespace Application.Common.Interfaces;
public interface IIdentityRoleServices
{
    Task<List<Permission>> GetRoles(CancellationToken cancellation);
}
