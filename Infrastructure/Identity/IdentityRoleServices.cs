using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Identity;
public class IdentityRoleServices: IIdentityRoleServices
{
    private readonly RoleManager<IdentityRole> _roleManager;
    public IdentityRoleServices(RoleManager<IdentityRole> roleManager)
    {
        _roleManager =roleManager;
    }

    public async Task<List<Permission>> GetRoles(CancellationToken cancellation)
    {
        var permissions = new List<Permission>();
        var roles = await _roleManager.Roles.ToListAsync(cancellation);
        foreach (var role in roles) 
        {
            permissions.Add(new Permission { Id = role.Id, Name = role.Name });
        }
        return permissions;
    }
}
