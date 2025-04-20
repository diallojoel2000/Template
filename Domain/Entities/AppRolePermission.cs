namespace Domain.Entities;
public class AppRolePermission
{
    public long AppRoleId { get; set; }
    public string PermissionId { get; set; } = null!;

    public virtual AppRole AppRole { get; set; } = null!;
}
