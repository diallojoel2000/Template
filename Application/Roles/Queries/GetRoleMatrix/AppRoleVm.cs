using Application.Common.Interfaces;
using Domain.Entities;

namespace Application.Roles;
public class AppRoleVm
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    private class Mapping : Profile
    {
        public Mapping() {
            CreateMap<AppRole, AppRoleVm>();
        }
    }
}
