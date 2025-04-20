using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using Domain.Entities;


namespace Application.Roles;
public class RoleMatrix
{
    public List<AppRoleVm> Roles { get; set; } = new List<AppRoleVm>();
    public List<Permission> Permissions { get; set; } = new List<Permission>();
    public List<RoleMapping> RoleMappings { get; set; } = new List<RoleMapping>();
}

public class RoleMapping:IMapFrom<AppRolePermission>
{
    public long RoleId { get; set; }
    public string? PermissionId { get; set; }

    public void Mapping(Profile profile, IEncryptionService encryptionService)
    {
        profile.CreateMap<AppRolePermission, RoleMapping>()
            .ForMember(m => m.RoleId, opt => opt.MapFrom(s => s.AppRoleId));

    }
}
