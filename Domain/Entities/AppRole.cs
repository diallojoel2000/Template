using Domain.Common;

namespace Domain.Entities;
public class AppRole:BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public virtual IList<AppRolePermission> RolePermissions { get; set; } = new List<AppRolePermission>();

}

